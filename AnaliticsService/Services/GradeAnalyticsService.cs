using AnaliticsService.DataAccess;
using AnaliticsService.DataAccess.Models;
using Analytics.Realisations;
using Infrastructure.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace AnaliticsService.Services;

public class GradeAnalyticsService : IGradeAnalyticsService
{
    private readonly AnalyticsDbContext _dbContext;
    private readonly UniversityMetrics _metrics;
    private readonly ILogger<GradeAnalyticsService> _logger;

    public GradeAnalyticsService(AnalyticsDbContext context,
        UniversityMetrics metrics,
        ILogger<GradeAnalyticsService> logger)
    {
        _dbContext = context;
        _metrics = metrics;
        _logger = logger;
    }

    public async Task<StudentStatistics> GetStudentStatisticsAsync(Guid studentId)
    {
        return await _dbContext.StudentStatistics
            .FirstOrDefaultAsync(ss => ss.StudentId == studentId)
            ?? new StudentStatistics { StudentId = studentId };

    }

    public async Task ProcessGradeEventAsync(GradeAddedEvent gradeEvent)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var gradeEventEntity = new GradeEvent
            {
                Id = Guid.NewGuid(),
                GradeId = gradeEvent.GradeId,
                StudentId = gradeEvent.StudentId,
                CourseId = gradeEvent.CourseId,
                TeacherId = gradeEvent.TeacherId,
                GradeValue = gradeEvent.GradeValue,
                Timestamp = gradeEvent.Timestamp
            };

            _dbContext.GradeEvents.Add(gradeEventEntity);
            await _dbContext.SaveChangesAsync();

            _metrics.GradeAdded(gradeEvent.CourseId, gradeEvent.GradeValue);

            await UpdateCourseStatisticsAsync(gradeEvent.CourseId);
            await UpdateStudentStatisticsAsync(gradeEvent.StudentId);

            _logger.LogInformation("Processed grade event: {GradeId}", gradeEvent.GradeId);

            _metrics.RecordGradeProcessingTime(stopwatch.Elapsed, true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing grade event: {GradeId}", gradeEvent.GradeId);
            _metrics.RecordGradeProcessingTime(stopwatch.Elapsed, false);
            throw;
        }
    }

    public async Task<CourseStatistics> GetCourseStatisticsAsync(Guid courseId)
    {
        return await _dbContext.CourseStatistics
            .FirstOrDefaultAsync(cs => cs.CourseId == courseId)
            ?? new CourseStatistics { CourseId = courseId, CourseName = $"Course {courseId}" };
    }

    public async Task<List<CourseStatistics>> GetTopCoursesAsync(int topCount = 10)
    {
        try
        {
            _logger.LogInformation($"Getting top {topCount} courses by average grade");

            var topCourses = await _dbContext.CourseStatistics
                .Where(cs => cs.TotalGrades >= 5)
                .OrderByDescending(cs => cs.AverageGrade)
                .ThenByDescending(cs => cs.TotalGrades)
                .Take(topCount)
                .ToListAsync();

            _logger.LogInformation($"Found {topCourses.Count} top courses");

            return topCourses;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting top courses");
            throw;
        }
    }

    public async Task UpdateStatisticsAsync()
    {
        try
        {
            _logger.LogInformation("Starting statistics update");

            // Обновляем статистику для всех курсов
            await UpdateAllCourseStatisticsAsync();

            // Обновляем статистику для всех студентов
            await UpdateAllStudentStatisticsAsync();

            _logger.LogInformation("Statistics update completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating statistics");
            throw;
        }
    }

    private async Task UpdateStudentStatisticsAsync(Guid studentId)
    {
        var stats = await _dbContext.GradeEvents
            .Where(ge => ge.StudentId == studentId)
            .GroupBy(ge => ge.StudentId)
            .Select(g => new
            {
                StudentId = g.Key,
                AverageGrade = g.Average(ge => ge.GradeValue),
                TotalGrades = g.Count(),
                ExcellentCount = g.Count(ge => ge.GradeValue == 5),
                GoodCount = g.Count(ge => ge.GradeValue == 4)
            })
            .FirstOrDefaultAsync();

        if (stats != null)
        {
            var studentStats = await _dbContext.StudentStatistics
                .FirstOrDefaultAsync(ss => ss.StudentId == studentId);

            if (studentStats == null)
            {
                studentStats = new StudentStatistics
                {
                    Id = Guid.NewGuid(),
                    StudentId = studentId
                };
                _dbContext.StudentStatistics.Add(studentStats);
            }

            studentStats.AverageGrade = Math.Round(stats.AverageGrade, 2);
            studentStats.TotalGrades = stats.TotalGrades;
            studentStats.ExcellentCount = stats.ExcellentCount;
            studentStats.GoodCount = stats.GoodCount;
            studentStats.LastUpdated = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

        }
    }

    private async Task UpdateCourseStatisticsAsync(Guid courseId)
    {
        var stats = await _dbContext.GradeEvents
            .Where(ge => ge.CourseId == courseId)
            .GroupBy(ge => ge.CourseId)
            .Select(g => new
            {
                CourseId = g.Key,
                AverageGrade = g.Average(ge => ge.GradeValue),
                TotalGrades = g.Count(),
                ExcellentCount = g.Count(ge => ge.GradeValue == 5),
                GoodCount = g.Count(ge => ge.GradeValue == 4),
                SatisfactoryCount = g.Count(ge => ge.GradeValue == 3),
                UnsatisfactoryCount = g.Count(ge => ge.GradeValue <= 2)
            })
            .FirstOrDefaultAsync();

        if (stats != null)
        {
            var courseStats = await _dbContext.CourseStatistics
                .FirstOrDefaultAsync(cs => cs.CourseId == courseId);

            if (courseStats == null)
            {
                courseStats = new CourseStatistics
                {
                    Id = Guid.NewGuid(),
                    CourseId = courseId,
                    CourseName = $"Course {courseId}"
                };
                _dbContext.CourseStatistics.Add(courseStats);
            }

            courseStats.AverageGrade = Math.Round(stats.AverageGrade, 2);
            courseStats.TotalGrades = stats.TotalGrades;
            courseStats.ExcellentCount = stats.ExcellentCount;
            courseStats.GoodCount = stats.GoodCount;
            courseStats.SatisfactoryCount = stats.SatisfactoryCount;
            courseStats.UnsatisfactoryCount = stats.UnsatisfactoryCount;
            courseStats.LastUpdated = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();
        }

    }

    private async Task UpdateAllCourseStatisticsAsync()
    {
        var courseIds = await _dbContext.GradeEvents
            .Select(ge => ge.CourseId)
            .Distinct()
            .ToListAsync();

        _logger.LogInformation($"Updating statistics for {courseIds.Count} courses");

        foreach (var courseId in courseIds)
        {
            await UpdateCourseStatisticsAsync(courseId);
        }
    }

    private async Task UpdateAllStudentStatisticsAsync()
    {
        var studentIds = await _dbContext.GradeEvents
            .Select(ge => ge.StudentId)
            .Distinct()
            .ToListAsync();

        _logger.LogInformation($"Updating statistics for {studentIds.Count} students");

        foreach (var studentId in studentIds)
        {
            await UpdateStudentStatisticsAsync(studentId);
        }
    }
}
