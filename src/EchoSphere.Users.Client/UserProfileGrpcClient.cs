using EchoSphere.GrpcModels;
using EchoSphere.SharedModels.Extensions;
using EchoSphere.Users.Abstractions;
using EchoSphere.Users.Abstractions.Models;
using EchoSphere.Users.Grpc;
using Google.Protobuf.WellKnownTypes;

namespace EchoSphere.Users.Client;

public sealed class UserProfileGrpcClient : IUserProfileService
{
	private readonly UserProfileService.UserProfileServiceClient _serviceGrpcClient;

	public UserProfileGrpcClient(UserProfileService.UserProfileServiceClient serviceGrpcClient)
	{
		_serviceGrpcClient = serviceGrpcClient;
	}

	public async Task<IReadOnlyList<UserProfile>> GetUserProfiles(CancellationToken cancellationToken)
	{
		var profiles = await _serviceGrpcClient.GetUserProfilesAsync(new Empty(), cancellationToken: cancellationToken);
		return profiles.Profiles
			.Select(x => new UserProfile
			{
				Id = IdValueExtensions.Parse<UserId>(x.Id),
				FirstName = x.FirstName,
				SecondName = x.SecondName,
			})
			.ToArray();
	}

	public async Task<Option<UserProfile>> GetUserProfile(UserId userId, CancellationToken cancellationToken)
	{
		var profile = await _serviceGrpcClient.GetUserProfileAsync(
			new UserIdDto { Value = userId.ToInnerString() }, cancellationToken: cancellationToken);
		return new UserProfile
		{
			Id = IdValueExtensions.Parse<UserId>(profile.Id),
			FirstName = profile.FirstName,
			SecondName = profile.SecondName,
		};
	}

	public async Task<IReadOnlyList<(UserId UserId, bool Exists)>> CheckUsersExistence(
		IReadOnlyList<UserId> userIds, CancellationToken cancellationToken)
	{
		var usersExistence = await _serviceGrpcClient.CheckUsersExistenceAsync(
			new UserIdsDto { Ids = { userIds.Select(x => x.ToInnerString()) } },
			cancellationToken: cancellationToken);

		return usersExistence.UsersExistence
			.Select(x => (IdValueExtensions.Parse<UserId>(x.UserId), x.Exists))
			.ToArray();
	}
}