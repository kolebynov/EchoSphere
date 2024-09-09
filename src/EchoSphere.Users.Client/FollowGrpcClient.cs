using EchoSphere.GrpcModels;
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

	public async ValueTask Follow(UserId followerUserId, UserId followUserId, CancellationToken cancellationToken)
	{
		await _serviceGrpcClient.FollowAsync(
			new FromToUserIds { FromUserId = followerUserId.Value.ToString(), ToUserId = followUserId.Value.ToString() },
			cancellationToken: cancellationToken);
	}

	public async ValueTask<IReadOnlyList<UserId>> GetFollowers(UserId userId, CancellationToken cancellationToken)
	{
		var followers = await _serviceGrpcClient.GetFollowersAsync(
			new UserIdDto { Value = userId.Value.ToString() }, cancellationToken: cancellationToken);
		return followers.Ids
			.Select(x => new UserId(Guid.Parse(x)))
			.ToArray();
	}
}