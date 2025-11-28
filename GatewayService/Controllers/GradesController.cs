using GradeServices;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static AuthServices.AuthService;
using static GradeServices.GradeService;

namespace GatewayService.Controllers;

[ApiController]
[Route("api/grades")]
public class GradesController : ControllerBase
{
    private readonly GradeServiceClient _gradeClient;
    private readonly AuthServiceClient _authClient;
    private readonly ILogger<GradesController> _logger;

    public GradesController(GradeServiceClient gradeClient,
        AuthServiceClient authClient,
        ILogger<GradesController> logger)
    {
        _gradeClient = gradeClient;
        _authClient = authClient;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> AddGrade([FromBody] AddGradeRequest request)
    {
        try
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

            var validationResponse = await _authClient.ValidateTokenAsync(
                new AuthServices.ValidateTokenRequest
                {
                    Token = token
                });

            if (!validationResponse.IsValid || validationResponse.Role != "teacher")
                return Forbid();

            var response = await _gradeClient.AddGradeAsync(new GradeServices.AddGradeRequest
            {
                StudentId = request.StudentId,
                CourseId = request.CourseId,
                GradeValue = request.GradeValue,
                TeacherId = validationResponse.UserId
            });

            return Ok(new
            {
                success = response.Success,
                grade_id = response.GradeId
            });
        }
        catch (RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.InvalidArgument)
        {
            return BadRequest(new
            {
                message = ex.Status.Detail
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error adding grade for student: {request.StudentId}");
            return StatusCode(500, new
            {
                message = "Internal server error"
            });
        }
    }

    [HttpGet("student/{studentId}")]
    public async Task<IActionResult> GetStudentGrades(string studentId)
    {
        try
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

            var validationResponse = await _authClient.ValidateTokenAsync(
                new AuthServices.ValidateTokenRequest
                {
                    Token = token
                });

            if (!validationResponse.IsValid)
                return Unauthorized();

            var response = await _gradeClient.GetStudentGradesAsync(
                new GetStudentGradesRequest
                {
                    StudentId = studentId
                });

            var grades = response.Grades.Select(g => new
            {
                id = g.Id,
                course_name = g.CourseName,
                grade_value = g.GradeValue,
                grade_date = g.GradeDate,
                teacher_name = g.TeacherName
            });

            return Ok(new
            {
                grades
            });
        }
        catch (RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.NotFound)
        {
            return NotFound(new
            {
                message = "Student not found"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting grades for student: {studentId}");
            return StatusCode(500, new
            {
                message = "Internal server error"
            });
        }
    }
}

public record AddGradeRequest(string StudentId, string CourseId, int GradeValue);
