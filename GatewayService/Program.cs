using Microsoft.AspNetCore.Builder;

var builder = WebApplication.CreateBuilder(args);
//builder.Services.AddSwaggerGen()
builder.Services.AddControllers();
//builder.Services.AddHttpClient(); AddSwagger

//builder.Services.AddGrpcClient<AuthServices.AuthServiceClient>(options =>)
//builder.Services.AddGrpcClient<AuthServices.UserServiceClient>(options =>)
//builder.Services.AddGrpcClient<AuthServices.GradeServiceClient>(options =>)

var app = builder.Build();

app.MapGet("/", () => "Hello World!");
app.MapControllers();
//app.UseSwagger();
//app.UseSwaggerUI();

app.Run();
