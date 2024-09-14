using EchoSphere.GrpcModels;
using EchoSphere.SharedModels.Extensions;
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

	public async Task<Option<IReadOnlyList<UserId>>> GetFriends(UserId userId, CancellationToken cancellationToken)
	{
		var friends = await _serviceGrpcClient.GetFriendsAsync(
			new UserIdDto { Value = userId.ToInnerString() }, cancellationToken: cancellationToken);
		return friends.Ids
			.Select(x => IdValueExtensions.Parse<UserId>(x))
			.ToArray();
	}

	public async Task<Either<SendFriendInviteError, Unit>> SendFriendInvite(UserId fromUserId, UserId toUserId, CancellationToken cancellationToken)
	{
		await _serviceGrpcClient.SendFriendInviteAsync(
			new FromToUserIds { FromUserId = fromUserId.ToInnerString(), ToUserId = toUserId.ToInnerString() },
			cancellationToken: cancellationToken);
		return Unit.Default;
	}

	public async Task<Option<IReadOnlyList<FriendInvitation>>> GetFriendInvites(UserId userId, CancellationToken cancellationToken)
	{
		var invites = await _serviceGrpcClient.GetFriendInvitesAsync(
			new UserIdDto { Value = userId.ToInnerString() }, cancellationToken: cancellationToken);

		return Some<IReadOnlyList<FriendInvitation>>(invites.Invitations
			.Select(x => new FriendInvitation
			{
				Id = IdValueExtensions.Parse<FriendInvitationId>(x.Id),
				FromUserId = IdValueExtensions.Parse<UserId>(x.FromUserId),
			})
			.ToArray());
	}

	public async Task<Either<FriendInviteError, Unit>> AcceptFriendInvite(FriendInvitationId invitationId, CancellationToken cancellationToken)
	{
		await _serviceGrpcClient.AcceptFriendInviteAsync(
			new FriendInvitationIdDto { Value = invitationId.ToInnerString() },
			cancellationToken: cancellationToken);
		return Unit.Default;
	}

	public async Task<Either<FriendInviteError, Unit>> RejectFriendInvite(FriendInvitationId invitationId, CancellationToken cancellationToken)
	{
		await _serviceGrpcClient.RejectFriendInviteAsync(
			new FriendInvitationIdDto { Value = invitationId.ToInnerString() },
			cancellationToken: cancellationToken);
		return Unit.Default;
	}
}