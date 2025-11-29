namespace AnaliticsService.DataAccess.Models;

public class CourseStatistics
{
    public Guid Id { get; set; }
    public Guid CourseId { get; set; }
    public string CourseName { get; set; }
    public double AverageGrade { get; set; }
    public int TotalGrades { get; set; }
    public int ExcellentCount { get; set; } // 5
    public int GoodCount { get; set; } // 4
    public int SatisfactoryCount { get; set; } // 3
    public int UnsatisfactoryCount { get; set; } // 1-2
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}

