namespace AnaliticsService.DataAccess.Models;

public class StudentStatistics
{
    public Guid Id { get; set; }
    public Guid StudentId { get; set; }
    public double AverageGrade { get; set; }
    public int TotalGrades { get; set; }
    public int ExcellentCount { get; set; }
    public int GoodCount { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

}
