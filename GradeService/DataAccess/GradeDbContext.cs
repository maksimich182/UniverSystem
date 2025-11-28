using GradeService.DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace GradeService.DataAccess;

public class GradeDbContext : DbContext
{
    public GradeDbContext(DbContextOptions<GradeDbContext> options) : base(options) { }

    public DbSet<Grade> Grades { get; set; }
    public DbSet<Course> Courses { get; set; }
    public DbSet<Student> Students { get; set; }
    public DbSet<Teacher> Teachers { get; set; }
}
