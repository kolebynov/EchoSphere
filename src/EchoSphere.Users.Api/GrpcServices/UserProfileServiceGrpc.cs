using EchoSphere.Domain.Abstractions.Extensions;
using EchoSphere.Domain.Abstractions.Models;
using EchoSphere.GrpcShared.Contracts;
using EchoSphere.GrpcShared.Errors;
using EchoSphere.GrpcShared.Extensions;
using EchoSphere.Users.Abstractions;
using EchoSphere.Users.Grpc;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;

namespace EchoSphere.Users.Api.GrpcServices;

[Authorize]
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
		_userProfileService.GetUserProfile(request.ToModel(), context.CancellationToken)
			.MapAsync(userProfile => new UserProfileDto
			{
				Id = userProfile.Id.ToInnerString(),
				FirstName = userProfile.FirstName,
				SecondName = userProfile.SecondName,
			})
			.IfNone(() => throw NotFoundError.Instance.ToStatusRpcException());

	public override Task<BasicUserProfileDto> GetBasicUserProfile(UserIdDto request, ServerCallContext context) =>
		_userProfileService.GetBasicUserProfile(request.ToModel(), context.CancellationToken)
			.MapAsync(userProfile => new BasicUserProfileDto
			{
				Id = userProfile.Id.ToInnerString(),
				FirstName = userProfile.FirstName,
				SecondName = userProfile.SecondName,
			})
			.IfNone(() => throw NotFoundError.Instance.ToStatusRpcException());

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
			.CheckUsersExistence(request.Ids.Select(IdValueExtensions.Parse<UserId>).ToArray(), context.CancellationToken)
			.Map(usersExistence => new CheckUsersExistenceResponse
			{
				UsersExistence =
				{
					usersExistence
						.Select(x => new UserExistence { UserId = x.UserId.ToInnerString(), Exists = x.Exists }),
				},
			});
}