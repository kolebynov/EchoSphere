using EchoSphere.GrpcModels;
using EchoSphere.Users.Abstractions;
using EchoSphere.Users.Abstractions.Models;
using EchoSphere.Users.Grpc;

namespace EchoSphere.Users.Client;

public sealed class FriendGrpcClient : IFriendService
{
	private readonly FriendService.FriendServiceClient _serviceGrpcClient;

	public FriendGrpcClient(FriendService.FriendServiceClient serviceGrpcClient)
	{
		_serviceGrpcClient = serviceGrpcClient;
	}

	public async ValueTask<IReadOnlyList<UserId>> GetFriends(UserId userId, CancellationToken cancellationToken)
	{
		var friends = await _serviceGrpcClient.GetFriendsAsync(
			new UserIdDto { Value = userId.Value.ToString() }, cancellationToken: cancellationToken);
		return friends.Ids
			.Select(x => new UserId(Guid.Parse(x)))
			.ToArray();
	}

	public async ValueTask SendFriendInvite(UserId fromUserId, UserId toUserId, CancellationToken cancellationToken)
	{
		await _serviceGrpcClient.SendFriendInviteAsync(
			new FromToUserIds { FromUserId = fromUserId.Value.ToString(), ToUserId = toUserId.Value.ToString() },
			cancellationToken: cancellationToken);
	}

	public async ValueTask<IReadOnlyList<UserId>> GetFriendInvites(UserId userId, CancellationToken cancellationToken)
	{
		var invites = await _serviceGrpcClient.GetFriendInvitesAsync(
			new UserIdDto { Value = userId.Value.ToString() }, cancellationToken: cancellationToken);
		return invites.Ids
			.Select(x => new UserId(Guid.Parse(x)))
			.ToArray();
	}

	public async ValueTask AcceptFriendInvite(UserId fromUserId, UserId toUserId, CancellationToken cancellationToken)
	{
		await _serviceGrpcClient.AcceptFriendInviteAsync(
			new FromToUserIds { FromUserId = fromUserId.Value.ToString(), ToUserId = toUserId.Value.ToString() },
			cancellationToken: cancellationToken);
	}

	public async ValueTask RejectFriendInvite(UserId fromUserId, UserId toUserId, CancellationToken cancellationToken)
	{
		await _serviceGrpcClient.RejectFriendInviteAsync(
			new FromToUserIds { FromUserId = fromUserId.Value.ToString(), ToUserId = toUserId.Value.ToString() },
			cancellationToken: cancellationToken);
	}
}