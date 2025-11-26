using Microsoft.EntityFrameworkCore;
using UserService.DataAccess.Models;

namespace UserService.DataAccess;

public class UserDbContext : DbContext
{
    public UserDbContext(DbContextOptions<UserDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Student> Students { get; set; }
    public DbSet<Teacher> Teachers { get; set; }
}
