using AnaliticsService.DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace AnaliticsService.DataAccess;

public class AnalyticsDbContext : DbContext
{
    public AnalyticsDbContext(DbContextOptions<AnalyticsDbContext> options) : base(options) { }

    public DbSet<GradeEvent> GradeEvents { get; set; }
    public DbSet<CourseStatistics> CourseStatistics { get; set; }
    public DbSet<StudentStatistics> StudentStatistics { get; set; }

}
