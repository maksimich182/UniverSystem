using AnaliticsService.DataAccess;
using AnaliticsService.Infrastructure.Kafka.Background;
using AnaliticsService.Infrastructure.Kafka;
using Microsoft.EntityFrameworkCore;
using Analytics.Realisations;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHostedService<GradeEventsConsumerService>();
builder.Services.AddSingleton<IKafkaConsumer, GradeEventsKafkaConsumer>();
builder.Services.AddSingleton<UniversityMetrics>();

builder.Services.AddDbContext<AnalyticsDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSQL")));


var app = builder.Build();

app.MapGet("/", () => "AnalyticsService");

app.Run();
