using Grpc.Core;
using UserServices;

namespace UserService.Services;

public class UserGrpcService : UserServices.UserService.UserServiceBase
{
    public override Task<GetUserProfileResponse> GetUserProfile(GetUserProfileRequest request, ServerCallContext context)
    {
        return base.GetUserProfile(request, context);
    }
}
