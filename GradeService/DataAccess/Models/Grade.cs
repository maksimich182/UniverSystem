namespace GradeService.DataAccess.Models;

public class Grade
{
    public Guid Id { get; set; }
    public Guid StudentId { get; set; }
    public Guid CourseId { get; set; }
    public int GradeValue { get; set; }
    public Guid TeacherId { get; set; }
    public DateTime GradeDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Student Student { get; set; }
    public Course Course { get; set; }
    public Teacher Teacher { get; set; }

}
