using Dusharp;
using EchoSphere.Users.Abstractions.Models;

namespace EchoSphere.Users.Abstractions;

[Union]
public partial class SendFriendInviteError
{
	[UnionCase]
	public static partial SendFriendInviteError InvalidFromUserId();

	[UnionCase]
	public static partial SendFriendInviteError InvalidToUserId();

	[UnionCase]
	public static partial SendFriendInviteError AlreadySent();

	[UnionCase]
	public static partial SendFriendInviteError AlreadyFriend();
}

[Union]
public partial class FriendInviteError
{
	[UnionCase]
	public static partial FriendInviteError InvalidInvitationId();
}

public interface IFriendService
{
	Task<Option<IReadOnlyList<UserId>>> GetFriends(UserId userId, CancellationToken cancellationToken);

	Task<Either<SendFriendInviteError, Unit>> SendFriendInvite(UserId fromUserId, UserId toUserId,
		CancellationToken cancellationToken);

	Task<Option<IReadOnlyList<FriendInvitation>>> GetFriendInvites(UserId userId, CancellationToken cancellationToken);

	Task<Either<FriendInviteError, Unit>> AcceptFriendInvite(
		FriendInvitationId invitationId, CancellationToken cancellationToken);

	Task<Either<FriendInviteError, Unit>> RejectFriendInvite(
		FriendInvitationId invitationId, CancellationToken cancellationToken);
}