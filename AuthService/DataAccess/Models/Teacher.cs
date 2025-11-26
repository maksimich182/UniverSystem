namespace AuthService.DataAccess.Models;

public class Teacher
{
    public Guid Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Department { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Guid UserId { get; set; }
    public User User { get; set; }
}

