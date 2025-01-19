using EchoSphere.Domain.Abstractions.Extensions;
using EchoSphere.Domain.Abstractions.Models;
using EchoSphere.GrpcClientShared;
using EchoSphere.GrpcShared.Errors;
using EchoSphere.GrpcShared.Extensions;
using EchoSphere.Users.Abstractions;
using EchoSphere.Users.Abstractions.Extensions;
using EchoSphere.Users.Abstractions.Models;
using EchoSphere.Users.Grpc;

namespace EchoSphere.Users.Client;

internal sealed class UserProfileGrpcClient : IUserProfileService
{
	private readonly GrpcCallExecutor<UserProfileService.UserProfileServiceClient> _grpcExecutor;

	public UserProfileGrpcClient(GrpcCallExecutor<UserProfileService.UserProfileServiceClient> grpcExecutor)
	{
		_grpcExecutor = grpcExecutor;
	}

	public Task<IReadOnlyList<UserProfile>> GetUserProfiles(CancellationToken cancellationToken) =>
		_grpcExecutor.ExecuteAsync<IReadOnlyList<UserProfile>>(
			async client =>
			{
				var profiles = await client.GetUserProfilesAsync(GrpcExtensions.EmptyInstance, cancellationToken: cancellationToken);
				return profiles.Profiles
					.Select(x => new UserProfile
					{
						Id = IdValueExtensions.Parse<UserId>(x.Id),
						FirstName = x.FirstName,
						SecondName = x.SecondName,
					})
					.ToArray();
			});

	public Task<Option<UserProfile>> GetUserProfile(UserId userId, CancellationToken cancellationToken) =>
		_grpcExecutor
			.ExecuteAsync<Option<UserProfile>, NotFoundError>(
				async client =>
				{
					var profile = await client.GetUserProfileAsync(userId.ToDto(), cancellationToken: cancellationToken);
					return new UserProfile
					{
						Id = IdValueExtensions.Parse<UserId>(profile.Id),
						FirstName = profile.FirstName,
						SecondName = profile.SecondName,
					};
				})
			.IfLeft(_ => None);

	public Task<Option<BasicUserProfile>> GetBasicUserProfile(UserId userId, CancellationToken cancellationToken) =>
		_grpcExecutor
			.ExecuteAsync<Option<BasicUserProfile>, NotFoundError>(
				async client =>
				{
					var profile = await client.GetBasicUserProfileAsync(userId.ToDto(), cancellationToken: cancellationToken);
					return new BasicUserProfile
					{
						Id = IdValueExtensions.Parse<UserId>(profile.Id),
						FirstName = profile.FirstName,
						SecondName = profile.SecondName,
					};
				})
			.IfLeft(_ => None);

	public Task<IReadOnlyList<(UserId UserId, bool Exists)>> CheckUsersExistence(
		IReadOnlyList<UserId> userIds, CancellationToken cancellationToken) =>
		_grpcExecutor.ExecuteAsync<IReadOnlyList<(UserId UserId, bool Exists)>>(
			async client =>
			{
				var usersExistence = await client.CheckUsersExistenceAsync(
					new UserIdsDto { Ids = { userIds.Select(x => x.ToInnerString()) } },
					cancellationToken: cancellationToken);

				return usersExistence.UsersExistence
					.Select(x => (IdValueExtensions.Parse<UserId>(x.UserId), x.Exists))
					.ToArray();
			});
}