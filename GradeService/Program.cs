using GradeService.Services;

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

builder.Services.AddGrpcReflection();
builder.Services.AddGrpc();

var app = builder.Build();

app.MapGrpcReflectionService();
app.MapGrpcService<GradeGrpcService>();

app.MapGet("/", () => "Grade Service!");

app.Run();
