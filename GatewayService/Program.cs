using AuthServices;
using GradeServices;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Npgsql;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Reflection;
using System.Text;
using UserServices;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));

    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "University API Gateway",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' [space] and then your token in the text input below."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var jaegerHost = builder.Configuration["Jaeger:Host"];
var jaegerPort = int.Parse(builder.Configuration["Jaeger:Port"]);

builder.Services.AddOpenTelemetry()
    .WithTracing(cfg =>
    cfg.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("gateway-service"))
    .AddAspNetCoreInstrumentation()
    .AddHttpClientInstrumentation(options =>
    {
        options.RecordException = true;
    })
    .AddNpgsql()
    .AddJaegerExporter(options =>
    {
        options.AgentHost = jaegerHost;
        options.AgentPort = jaegerPort;
        options.Protocol = JaegerExportProtocol.UdpCompactThrift;
        options.ExportProcessorType = ExportProcessorType.Simple;
    }));

builder.Services.AddControllers();

builder.Services.AddGrpcClient<AuthService.AuthServiceClient>(options =>
{
    options.Address = new Uri(builder.Configuration["Services:AuthService"]);
});

builder.Services.AddGrpcClient<UserService.UserServiceClient>(options =>
{
    options.Address = new Uri(builder.Configuration["Services:UserService"]);
});

builder.Services.AddGrpcClient<GradeService.GradeServiceClient>(options =>
{
    options.Address = new Uri(builder.Configuration["Services:GradeService"]);
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/", () => "GatewayService");
app.MapControllers();


app.Run();
