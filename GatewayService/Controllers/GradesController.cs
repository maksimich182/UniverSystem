using GradeServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static GradeServices.GradeService;

namespace GatewayService.Controllers;

[ApiController]
[Route("api/grades")]
public class GradesController : ControllerBase
{
    private readonly GradeServiceClient _gradeClient;
    private readonly ILogger<GradesController> _logger;

    public GradesController(GradeServiceClient gradeClient)
    {
        _gradeClient = gradeClient;
    }

    [HttpPost]
    public async Task<IActionResult> AddGrade([FromBody] AddGradeRequest request)
    {
        await _gradeClient.AddGradeAsync(request);
        return Ok();
    }

    [HttpGet("student/{studentId}")]
    public async Task<IActionResult> GetStudentGrades(string studentId)
    {
        await _gradeClient.GetStudentGradesAsync(new GetStudentGradesRequest { StudentId = studentId });
        return Ok();
    }
}
