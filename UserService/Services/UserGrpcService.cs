using Grpc.Core;
using Infrastructure.Abstractions;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using UserService.DataAccess;
using UserServices;

namespace UserService.Services;

public class UserGrpcService : UserServices.UserService.UserServiceBase
{
    private readonly UserDbContext _dbContext;
    private readonly IRedisService _redisService;
    private readonly ILogger<UserGrpcService> _logger;

    public UserGrpcService(UserDbContext dbContext, 
        IRedisService redisService, 
        ILogger<UserGrpcService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
        _redisService = redisService;
    }

    public override async Task<GetUserProfileResponse> GetUserProfile(
        GetUserProfileRequest request,
        ServerCallContext context)
    {

        var cachedKey = $"user_profile:{request.UserId}";

        var cachedProfile = await _redisService.GetAsync<GetUserProfileResponse>(cachedKey);

        if (cachedProfile != null)
        {
            _logger.LogInformation($"Returning cached profile for user: {request.UserId}");
            return cachedProfile;
        }

        var userId = Guid.Parse(request.UserId);

        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "User not found"));
        }

        var response = new GetUserProfileResponse
        {
            User = new User
            {
                Id = user.Id.ToString(),
                Username = user.Username,
                Email = user.Email,
                Role = user.Role
            }
        };

        switch (user.Role)
        {
            case "student":
                var student = await _dbContext.Students
                    .FirstOrDefaultAsync(s => s.UserId == userId);

                if (student != null)
                {
                    response.Student = new StudentProfile
                    {
                        StudentId = student.Id.ToString(),
                        FirstName = student.FirstName,
                        LastName = student.LastName,
                        GroupName = student.GroupName,
                        Faculty = student.Faculty
                    };
                }
                break;
            case "teacher":
                var teacher = await _dbContext.Teachers.FirstOrDefaultAsync(t => t.UserId == userId);

                if (teacher != null)
                {
                    response.Teacher = new TeacherProfile
                    {
                        TeacherId = teacher.Id.ToString(),
                        FirstName = teacher.FirstName,
                        LastName = teacher.LastName,
                        Department = teacher.Department
                    };
                }
                break;
            default:
                throw new RpcException(new Status(StatusCode.NotFound, "Role not found"));
        }

        await _redisService.SetAsync(cachedKey, response, TimeSpan.FromMinutes(30));
        _logger.LogInformation($"Profile loaded from database for user: {request.UserId}");

        return response;
    }
}
