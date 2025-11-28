namespace GradeService.DataAccess.Models;

public class Course
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public Guid? TeacherId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Teacher Teacher { get; set; }
    public ICollection<Grade> Grades { get; set; }

}
