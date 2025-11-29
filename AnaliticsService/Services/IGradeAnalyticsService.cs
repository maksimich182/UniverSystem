using AnaliticsService.DataAccess.Models;
using Infrastructure.Events;

namespace AnaliticsService.Services;

public interface IGradeAnalyticsService
{
    Task ProcessGradeEventAsync(GradeAddedEvent gradeEvent);
    Task<CourseStatistics> GetCourseStatisticsAsync(Guid courseId);
    Task<StudentStatistics> GetStudentStatisticsAsync(Guid studentId);
    Task<List<CourseStatistics>> GetTopCoursesAsync(int topCount = 10);
    Task UpdateStatisticsAsync();
}
