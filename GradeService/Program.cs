using GradeService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpcReflection();
builder.Services.AddGrpc();

var app = builder.Build();

app.MapGrpcReflectionService();
app.MapGrpcService<GradeGrpcService>();

app.MapGet("/", () => "Grade Service!");

app.Run();
