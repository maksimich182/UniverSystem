using Microsoft.AspNetCore.Mvc;
using UserServices;
using AuthServices;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;

namespace GatewayService.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly UserService.UserServiceClient _userClient;
    private readonly AuthService.AuthServiceClient _authClient;
    private readonly ILogger<UsersController> _logger;

    public UsersController(UserService.UserServiceClient userClient,
        AuthServices.AuthService.AuthServiceClient authClient,
        ILogger<UsersController> logger)
    {
        _userClient = userClient;
        _authClient = authClient;
        _logger = logger;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(string id)
    {
        try
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

            if (string.IsNullOrEmpty(token))
                return Unauthorized();

            var validationResponse = await _authClient.ValidateTokenAsync(
                new ValidateTokenRequest
                {
                    Token = token
                });
            if (!validationResponse.IsValid)
                return Unauthorized();

            var response = await _userClient.GetUserProfileAsync(
                new GetUserProfileRequest
                {
                    UserId = id
                });

            object profile = null;
            switch (response.ProfileCase)
            {
                case GetUserProfileResponse.ProfileOneofCase.Student:
                    profile = new
                    {
                        student_id = response.Student.StudentId,
                        first_name = response.Student.FirstName,
                        last_name = response.Student.LastName,
                        group_name = response.Student.GroupName,
                        faculty = response.Student.Faculty
                    };
                    break;
                case GetUserProfileResponse.ProfileOneofCase.Teacher:
                    profile = new
                    {
                        teacher_id = response.Teacher.TeacherId,
                        first_name = response.Teacher.FirstName,
                        last_name = response.Teacher.LastName,
                        department = response.Teacher.Department
                    };
                    break;
                default:
                    throw new Exception("Role invalid");
            }

            return Ok(new
            {
                user = new
                {
                    id = response.User.Id,
                    username = response.User.Username,
                    email = response.User.Email,
                    role = response.User.Role
                },
                profile = profile
            });
        }
        catch (RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.NotFound)
        {
            return NotFound(new { message = "User not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting profile for user: {id}");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

}
