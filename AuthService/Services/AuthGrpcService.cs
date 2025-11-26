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

    private string GenerateJwtToken(DataAccess.Models.User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Secret"]);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new System.Security.Claims.ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role)
            }),
            Expires = DateTime.UtcNow.AddHours(2),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
