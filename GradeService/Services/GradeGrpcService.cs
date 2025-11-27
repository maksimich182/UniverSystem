using GradeServices;
using Grpc.Core;

namespace GradeService.Services;

public class GradeGrpcService : GradeServices.GradeService.GradeServiceBase
{

    public override Task<AddGradeResponse> AddGrade(AddGradeRequest request, ServerCallContext context)
    {
        return base.AddGrade(request, context);
    }

    public override Task<GetStudentGradesResponse> GetStudentGrades(GetStudentGradesRequest request, ServerCallContext context)
    {
        return base.GetStudentGrades(request, context);
    }
}
