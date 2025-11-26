using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using UserService.DataAccess;
using UserServices;

namespace UserService.Services;

public class UserGrpcService : UserServices.UserService.UserServiceBase
{
    private readonly UserDbContext _dbContext;
    private readonly ILogger<UserGrpcService> _logger;

    public UserGrpcService(UserDbContext dbContext, ILogger<UserGrpcService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public override async Task<GetUserProfileResponse> GetUserProfile(
        GetUserProfileRequest request, 
        ServerCallContext context)
    {

        //Redis

        var userId = Guid.Parse(request.UserId);

        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);

        if(user == null)
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

                if(student != null)
                {
                    response.Student = new StudentProfile
                    {
                        FirstName = student.FirstName,
                        LastName = student.LastName,
                        GroupName = student.GroupName,
                        Faculty = student.Faculty
                    };
                }
                break;
            case "teacher":
                var teacher = await _dbContext.Teachers.FirstOrDefaultAsync(t => t.UserId == userId);

                if(teacher != null)
                {
                    response.Teacher = new TeacherProfile
                    {
                        FirstName = teacher.FirstName,
                        LastName = teacher.LastName,
                        Department = teacher.Department
                    };
                }
                break;
            default:
                throw new RpcException(new Status(StatusCode.NotFound, "Role not found"));
        }

        //Redis

        _logger.LogInformation($"Profile loaded from database for user: {request.UserId}");

        return response;
    }
}
