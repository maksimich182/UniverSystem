using AnaliticsService.DataAccess;
using AnaliticsService.Infrastructure.Kafka.Background;
using AnaliticsService.Infrastructure.Kafka;
using Microsoft.EntityFrameworkCore;
using Analytics.Realisations;
using Serilog;
using Prometheus;
using AnaliticsService.Services;
using Serilog.Sinks.Graylog;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

var graylogHost = builder.Configuration["Graylog:Host"];
var graylogPort = int.Parse(builder.Configuration["Graylog:Port"]);


Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.Graylog(graylogHost, graylogPort,
        transportType: Serilog.Sinks.Graylog.Core.Transport.TransportType.Udp)
    .CreateLogger();

builder.Services.AddSerilog();

builder.Services.AddOpenTelemetry()
    .WithTracing(cfg => 
    cfg.AddAspNetCoreInstrumentation()
    .AddConsoleExporter());

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHostedService<GradeEventsConsumerService>();
builder.Services.AddScoped<IKafkaConsumer, GradeEventsKafkaConsumer>();

builder.Services.AddMetrics()
    .AddScoped<UniversityMetrics>();

builder.Services.AddScoped<IGradeAnalyticsService, GradeAnalyticsService>();


builder.Services.AddDbContext<AnalyticsDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSQL"))
    .UseSnakeCaseNamingConvention());


var app = builder.Build();

app.MapControllers();
app.MapMetrics();

app.MapGet("/", () => "AnalyticsService");

app.Run();
