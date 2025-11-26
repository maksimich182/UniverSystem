using AuthServices;
using Grpc.Core;
using Microsoft.AspNetCore.Mvc;
using static AuthServices.AuthService;

namespace GatewayService.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthServiceClient _authClient;
    private readonly ILogger<AuthController> _logger;

    public AuthController(AuthServiceClient authClient, ILogger<AuthController> logger)
    {
        _authClient = authClient;
        _logger = logger;
    }

    /// <summary>
    /// Проверка Swagger
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    /// <response code = "400" > Одного или нескольких регионов нет в системе</response>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var response = await _authClient.LoginAsync(new AuthServices.LoginRequest
            {
                Username = request.Username,
                Password = request.Password
            });

            return Ok(new
            {
                token =response.Token,
                user = new
                {
                    id = response.User.Id,
                    username = response.User.Username,
                    email = response.User.Email,
                    role = response.User.Role
                }
            });
        }
        catch (RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.Unauthenticated)
        {
            return Unauthorized(new
            {
                message = "Invalid credentials"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error during login for user: {request.Username}");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }
}

/// <summary>
/// Запрос авторизации
/// </summary>
/// <param name="Username">Имя пользователя</param>
/// <param name="Password">Пароль</param>
public record LoginRequest(string Username, string Password);