using GradeService.DataAccess;
using GradeService.Services;
using Infrastructure.Abstractions;
using Infrastructure.Realisations;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

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
