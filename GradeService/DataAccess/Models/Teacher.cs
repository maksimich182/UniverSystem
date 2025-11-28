namespace GradeService.DataAccess.Models;

public class Teacher
{
    public Guid Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Department { get; set; }

    public ICollection<Course> Courses { get; set; }
    public ICollection<Grade> Grades { get; set; }

}
