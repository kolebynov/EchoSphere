using EchoSphere.GrpcModels;
using EchoSphere.Users.Abstractions;
using EchoSphere.Users.Abstractions.Models;
using EchoSphere.Users.Grpc;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace EchoSphere.Users.Api.GrpcServices;

internal sealed class UserProfileServiceGrpc : UserProfileService.UserProfileServiceBase
{
	private readonly ILogger<UserProfileServiceGrpc> _logger;
	private readonly IUserProfileService _userProfileService;

	public UserProfileServiceGrpc(ILogger<UserProfileServiceGrpc> logger, IUserProfileService userProfileService)
	{
		_logger = logger;
		_userProfileService = userProfileService;
	}

	public override async Task<UserProfileDto> GetUserProfile(UserIdDto request, ServerCallContext context)
	{
		var userProfile = await _userProfileService.GetUserProfile(new UserId(Guid.Parse(request.Value)), context.CancellationToken);
		return new UserProfileDto
		{
			Id = userProfile.Id.Value.ToString(),
			FirstName = userProfile.FirstName,
			SecondName = userProfile.SecondName,
		};
	}

	public override async Task<GetUserProfilesResponse> GetUserProfiles(Empty request, ServerCallContext context)
	{
		var userProfiles = await _userProfileService.GetUserProfiles(context.CancellationToken);
		return new GetUserProfilesResponse
		{
			Profiles =
			{
				userProfiles.Select(x => new UserProfileDto
				{
					Id = x.Id.Value.ToString(),
					FirstName = x.FirstName,
					SecondName = x.SecondName,
				}),
			},
		};
	}
}