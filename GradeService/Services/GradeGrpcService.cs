using GradeService.DataAccess;
using GradeService.DataAccess.Models;
using GradeServices;
using Grpc.Core;
using Infrastructure.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace GradeService.Services;

public class GradeGrpcService : GradeServices.GradeService.GradeServiceBase
{
    private readonly GradeDbContext _dbContext;
    private readonly IKafkaProducer _kafkaProducer;
    private readonly ILogger<GradeGrpcService> _logger;

    public GradeGrpcService(GradeDbContext dbContext,
        IKafkaProducer kafkaProducer,
        ILogger<GradeGrpcService> logger)
    {
        _dbContext = dbContext;
        _kafkaProducer = kafkaProducer;
        _logger = logger;
    }

    public override async Task<AddGradeResponse> AddGrade(AddGradeRequest request, ServerCallContext context)
    {
        _logger.LogInformation($"Adding grade for student: {request.StudentId}");

        if(request.GradeValue < 1 || request.GradeValue > 5)
        {
            throw new RpcException(
                new Status(StatusCode.InvalidArgument, "Grade must be between 1 and 5"));
        }

        var grade = new DataAccess.Models.Grade
        {
            Id = Guid.NewGuid(),
            StudentId = Guid.Parse(request.StudentId),
            CourseId = Guid.Parse(request.CourseId),
            GradeValue = request.GradeValue,
            TeacherId = Guid.Parse(request.TeacherId),
            GradeDate = DateTime.UtcNow
        };

        _dbContext.Grades.Add(grade);
        await _dbContext.SaveChangesAsync();

        await _kafkaProducer.ProduceAsync("grade-events", new
        {
            GradeId = grade.Id,
            StudentId = grade.StudentId,
            CourseId = grade.CourseId,
            GradeValue = grade.GradeValue,
            TeacherId = grade.TeacherId,
            TimeStamp = DateTime.UtcNow
        });

        _logger.LogInformation($"Grade add successfully: {grade.Id}");

        return new AddGradeResponse
        {
            Success = true,
            GradeId = grade.Id.ToString()
        };
    }

    public override async Task<GetStudentGradesResponse> GetStudentGrades(GetStudentGradesRequest request, ServerCallContext context)
    {
        var grades = await _dbContext.Grades
            .Include(g => g.Course)
            .Include(g => g.Teacher)
            .Where(g => g.StudentId == Guid.Parse(request.StudentId))
            .OrderByDescending(g => g.GradeDate)
            .Select(g => new GradeServices.Grade
            {
                Id = g.Id.ToString(),
                CourseName = g.Course.Name,
                GradeValue = g.GradeValue,
                GradeDate = g.GradeDate.ToString("yyy-MM-dd"),
                TeacherName = $"{g.Teacher.FirstName} {g.Teacher.LastName}"
            })
            .ToListAsync();

        var response = new GetStudentGradesResponse();
        response.Grades.AddRange(grades);

        return response;
    }
}
