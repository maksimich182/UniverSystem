using Infrastructure.Abstractions;
using Infrastructure.Realisations;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using UserService.DataAccess;
using UserService.Services;

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

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis")));
builder.Services.AddScoped<IRedisService, RedisService>();

builder.Services.AddDbContext<UserDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSQL"))
        .UseSnakeCaseNamingConvention());

builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();

var app = builder.Build();

app.MapGrpcReflectionService();
app.MapGrpcService<UserGrpcService>();


app.MapGet("/", () => "UserService");

app.Run();
