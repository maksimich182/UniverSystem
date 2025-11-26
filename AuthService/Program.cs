using AuthService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    // Настраиваем endpoint для gRPC
    options.ListenAnyIP(int.Parse(builder.Configuration["Ports:Grpc"]), listenOptions =>
    {
        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2;
    });

    // Опционально: можно добавить HTTP для тестов
    options.ListenAnyIP(int.Parse(builder.Configuration["Ports:Http"]), listenOptions =>
    {
        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1;
    });
});

builder.Services.AddGrpcReflection();
builder.Services.AddGrpc();

var app = builder.Build();

app.MapGrpcReflectionService();
app.MapGrpcService<AuthGrpcService>();
app.MapGet("/", () => "Hello World!");

app.Run();
