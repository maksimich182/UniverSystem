namespace AnaliticsService.DataAccess.Models;

public class GradeEvent
{
    public Guid Id { get; set; }
    public Guid GradeId { get; set; }
    public Guid StudentId { get; set; }
    public Guid CourseId { get; set; }
    public Guid TeacherId { get; set; }
    public int GradeValue { get; set; }
    public DateTime Timestamp { get; set; }
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;

}
