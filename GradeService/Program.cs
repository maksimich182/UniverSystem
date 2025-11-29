using GradeService.DataAccess;
using GradeService.Services;
using Infrastructure.Abstractions;
using Infrastructure.Realisations;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Sinks.Graylog;

var builder = WebApplication.CreateBuilder(args);

var graylogHost = builder.Configuration["Graylog:Host"];
var graylogPort = int.Parse(builder.Configuration["Graylog:Port"]);

var jaegerHost = builder.Configuration["Jaeger:Host"];
var jaegerPort = int.Parse(builder.Configuration["Jaeger:Port"]);


Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.Graylog(graylogHost, graylogPort,
        transportType: Serilog.Sinks.Graylog.Core.Transport.TransportType.Udp)
    .CreateLogger();

builder.Services.AddSerilog();

builder.Services.AddOpenTelemetry()
    .WithTracing(cfg =>
    cfg.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("grade-service"))
    .AddAspNetCoreInstrumentation()
    .AddNpgsql()
    .AddJaegerExporter(options =>
    {
        options.AgentHost = jaegerHost;
        options.AgentPort = jaegerPort;
        options.Protocol = JaegerExportProtocol.UdpCompactThrift;
        options.ExportProcessorType = ExportProcessorType.Simple;
    }));

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(int.Parse(builder.Configuration["Ports:Grpc"]), listenOptions =>
    {
        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2;
    });

    options.ListenAnyIP(int.Parse(builder.Configuration["Ports:Http"]), listenOptions =>
    {
        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1;
    });
});

builder.Services.AddScoped<IKafkaProducer, KafkaProducer>();

builder.Services.AddDbContext<GradeDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSQL"))
    .UseSnakeCaseNamingConvention());

builder.Services.AddGrpcReflection();
builder.Services.AddGrpc();

var app = builder.Build();

app.MapGrpcReflectionService();
app.MapGrpcService<GradeGrpcService>();

app.MapGet("/", () => "Grade Service!");

app.Run();
