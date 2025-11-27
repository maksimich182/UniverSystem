using AuthService.DataAccess;
using AuthServices;
using Grpc.Core;
using Infrastructure.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AuthService.Services;

public class AuthGrpcService : AuthServices.AuthService.AuthServiceBase
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthGrpcService> _logger;
    private readonly AuthDbContext _dbContext;
    private readonly IRedisService _redisService;

    public AuthGrpcService(IConfiguration configuration,
        AuthDbContext dbContext,
        IRedisService redisService,
        ILogger<AuthGrpcService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _dbContext = dbContext;

        var jwtSecret = _configuration["Jwt:Secret"];
        if (string.IsNullOrEmpty(jwtSecret))
        {
            throw new InvalidOperationException("JWT Secret is not configured");
        }

        _logger.LogInformation("AuthGrpcService initialized with JWT issuer: {Issuer}",
            _configuration["Jwt:Issuer"]);
        _redisService = redisService;
    }

    public override async Task<LoginResponse> Login(LoginRequest request, ServerCallContext context)
    {
        
        _logger.LogInformation($"Login attempt for user: {request.Username}");

        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == request.Username && u.IsActive);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid credentioals"));
        }

        var token = GenerateJwtToken(user);

        await _redisService.SetAsync($"session:{user.Id}", token, TimeSpan.FromHours(2));

        return new LoginResponse
        {
            Token = token,
            User = new User
            {
                Id = user.Id.ToString(),
                Username = user.Username,
                Email = user.Email,
                Role = user.Role
            }
        };
    }

    public override async Task<ValidateTokenResponse> ValidateToken(ValidateTokenRequest request, ServerCallContext context)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Secret"]);

            tokenHandler.ValidateToken(request.Token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;
            var userId = jwtToken.Claims.First(x => x.Type == JwtRegisteredClaimNames.Sub).Value;

            var cachedToken = await _redisService.GetAsync<string>($"session:{userId}");
            if (cachedToken != request.Token)
            {
                return new ValidateTokenResponse { IsValid = false };
            }

            var user = await _dbContext.Users.FindAsync(Guid.Parse(userId));
            if (user == null || !user.IsActive)
                return new ValidateTokenResponse { IsValid = false };

            return new ValidateTokenResponse
            {
                IsValid = true,
                UserId = userId,
                Role = user.Role
            };
        }
        catch
        {
            return new ValidateTokenResponse { IsValid = false };
        }
    }


    private string GenerateJwtToken(DataAccess.Models.User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Secret"]);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),        
                    new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),     
                    new Claim(ClaimTypes.Role, user.Role),                           
                    new Claim(JwtRegisteredClaimNames.Email, user.Email)             
                }),
            Expires = DateTime.UtcNow.AddHours(2),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
        SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
