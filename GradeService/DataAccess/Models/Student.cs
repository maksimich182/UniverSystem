namespace GradeService.DataAccess.Models;

public class Student
{
    public Guid Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string GroupName { get; set; }
    public string Faculty { get; set; }

    public ICollection<Grade> Grades { get; set; }
}
