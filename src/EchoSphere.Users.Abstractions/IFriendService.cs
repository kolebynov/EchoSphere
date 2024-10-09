using Dusharp;
using EchoSphere.Domain.Abstractions.Models;
using EchoSphere.Users.Abstractions.Models;

namespace EchoSphere.Users.Abstractions;

[Union]
public partial struct SendFriendInviteError
{
	[UnionCase]
	public static partial SendFriendInviteError CurrentUserNotFound();

	[UnionCase]
	public static partial SendFriendInviteError ToUserNotFound();

	[UnionCase]
	public static partial SendFriendInviteError AlreadySent();

	[UnionCase]
	public static partial SendFriendInviteError AlreadyFriend();
}

[Union]
public partial struct FriendInviteError
{
	[UnionCase]
	public static partial FriendInviteError InvitationNotFound();
}

public interface IFriendService
{
	Task<Option<IReadOnlyList<UserId>>> GetFriends(UserId userId, CancellationToken cancellationToken);

	Task<Either<SendFriendInviteError, Unit>> SendFriendInvite(UserId toUserId, CancellationToken cancellationToken);

	Task<Option<IReadOnlyList<FriendInvitation>>> GetCurrentUserFriendInvites(CancellationToken cancellationToken);

	Task<Either<FriendInviteError, Unit>> AcceptFriendInvite(
		FriendInvitationId invitationId, CancellationToken cancellationToken);

	Task<Either<FriendInviteError, Unit>> RejectFriendInvite(
		FriendInvitationId invitationId, CancellationToken cancellationToken);
}