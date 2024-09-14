using EchoSphere.GrpcModels;
using EchoSphere.SharedModels.Extensions;
using EchoSphere.Users.Abstractions;
using EchoSphere.Users.Abstractions.Models;
using EchoSphere.Users.Grpc;

namespace EchoSphere.Users.Client;

public sealed class FollowGrpcClient : IFollowService
{
	private readonly FollowService.FollowServiceClient _serviceGrpcClient;

	public FollowGrpcClient(FollowService.FollowServiceClient serviceGrpcClient)
	{
		_serviceGrpcClient = serviceGrpcClient;
	}

	public async Task<Either<FollowError, Unit>> Follow(UserId followerUserId, UserId followUserId,
		CancellationToken cancellationToken)
	{
		await _serviceGrpcClient.FollowAsync(
			new FromToUserIds { FromUserId = followerUserId.ToInnerString(), ToUserId = followUserId.ToInnerString() },
			cancellationToken: cancellationToken);
		return Unit.Default;
	}

	public async Task<Option<IReadOnlyList<UserId>>> GetFollowers(UserId userId, CancellationToken cancellationToken)
	{
		var followers = await _serviceGrpcClient.GetFollowersAsync(
			new UserIdDto { Value = userId.ToInnerString() }, cancellationToken: cancellationToken);
		return followers.Ids
			.Select(IdValueExtensions.Parse<UserId>)
			.ToArray();
	}
}