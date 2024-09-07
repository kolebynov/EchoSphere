using EchoSphere.Users.Api.Grpc;
using Grpc.Core;

namespace EchoSphere.Users.Api.Services;

internal sealed class UsersService : Grpc.UsersService.UsersServiceBase
{
	private readonly ILogger<UsersService> _logger;

	public UsersService(ILogger<UsersService> logger)
	{
		_logger = logger;
	}

	public override async Task<CreateUserResponse> CreateUser(CreateUserRequest request, ServerCallContext context)
	{
		return new CreateUserResponse { Id = Guid.NewGuid().ToString("D") };
	}

	public override async Task<GetUserProfilesResponse> GetUserProfiles(
		GetUserProfilesRequest request, ServerCallContext context)
	{
		return new GetUserProfilesResponse();
	}

	public override async Task<GetUserByUserNameAndPasswordResponse> GetUserByUserNameAndPassword(
		GetUserByUserNameAndPasswordRequest request, ServerCallContext context)
	{
		return new GetUserByUserNameAndPasswordResponse { User = null };
	}
}