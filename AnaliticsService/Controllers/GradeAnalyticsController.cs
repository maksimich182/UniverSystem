using AnaliticsService.Services;
using Microsoft.AspNetCore.Mvc;

namespace AnaliticsService.Controllers;

[ApiController]
[Route("api/analytics/grades")]
public class GradeAnalyticsController : ControllerBase
{
    private readonly IGradeAnalyticsService _gradeAnalyticsService;
    private readonly ILogger<GradeAnalyticsController> _logger;

    public GradeAnalyticsController(IGradeAnalyticsService gradeAnalyticsService, ILogger<GradeAnalyticsController> logger)
    {
        _gradeAnalyticsService = gradeAnalyticsService;
        _logger = logger;
    }

    [HttpGet("courses/{courseId}")]
    public async Task<IActionResult> GetCourseStatistics(Guid courseId)
    {
        try
        {
            var statistics = await _gradeAnalyticsService.GetCourseStatisticsAsync(courseId);
            return Ok(statistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting course statistics for {CourseId}", courseId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpGet("students/{studentId}")]
    public async Task<IActionResult> GetStudentStatistics(Guid studentId)
    {
        try
        {
            var statistics = await _gradeAnalyticsService.GetStudentStatisticsAsync(studentId);
            return Ok(statistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting student statistics for {StudentId}", studentId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpGet("courses/top")]
    public async Task<IActionResult> GetTopCourses([FromQuery] int top = 10)
    {
        try
        {
            var topCourses = await _gradeAnalyticsService.GetTopCoursesAsync(top);
            return Ok(topCourses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting top courses");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpPost("statistics/update")]
    public async Task<IActionResult> UpdateStatistics()
    {
        try
        {
            await _gradeAnalyticsService.UpdateStatisticsAsync();
            return Ok(new { message = "Statistics updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating statistics");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }
}

