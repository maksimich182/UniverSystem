namespace AuthService.DataAccess.Models;

public class Student
{
    public Guid Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string GroupName { get; set; }
    public string Faculty { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Guid UserId { get; set; }
    public User User { get; set; }
}

