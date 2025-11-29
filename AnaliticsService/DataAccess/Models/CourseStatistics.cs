namespace AnaliticsService.DataAccess.Models;

public class CourseStatistics
{
    public Guid Id { get; set; }
    public Guid CourseId { get; set; }
    public string CourseName { get; set; }
    public double AverageGrade { get; set; }
    public int TotalGrades { get; set; }
    public int ExcellentCount { get; set; }
    public int GoodCount { get; set; }
    public int SatisfactoryCount { get; set; }
    public int UnsatisfactoryCount { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}

