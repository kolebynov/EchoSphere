using EchoSphere.GrpcModels;
using EchoSphere.SharedModels.Extensions;
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

	public override Task<UserProfileDto> GetUserProfile(UserIdDto request, ServerCallContext context) =>
		_userProfileService.GetUserProfile(IdValueExtensions.Parse<UserId>(request.Value), context.CancellationToken)
			.Map(userProfileOpt => userProfileOpt
				.Map(userProfile => new UserProfileDto
				{
					Id = userProfile.Id.ToInnerString(),
					FirstName = userProfile.FirstName,
					SecondName = userProfile.SecondName,
				})
				.IfNone(() => throw new RpcException(new Status(StatusCode.NotFound, "User not found."))));

	public override async Task<GetUserProfilesResponse> GetUserProfiles(Empty request, ServerCallContext context)
	{
		var userProfiles = await _userProfileService.GetUserProfiles(context.CancellationToken);
		return new GetUserProfilesResponse
		{
			Profiles =
			{
				userProfiles.Select(x => new UserProfileDto
				{
					Id = x.Id.ToInnerString(),
					FirstName = x.FirstName,
					SecondName = x.SecondName,
				}),
			},
		};
	}

	public override Task<CheckUsersExistenceResponse> CheckUsersExistence(UserIdsDto request, ServerCallContext context) =>
		_userProfileService
			.CheckUsersExistence(request.Ids.Select(x => IdValueExtensions.Parse<UserId>(x)).ToArray(), context.CancellationToken)
			.Map(usersExistence => new CheckUsersExistenceResponse
			{
				UsersExistence =
				{
					usersExistence
						.Select(x => new UserExistence { UserId = x.UserId.ToInnerString(), Exists = x.Exists }),
				},
			});
}