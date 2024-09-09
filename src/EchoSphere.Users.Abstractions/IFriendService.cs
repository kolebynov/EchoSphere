using EchoSphere.Users.Abstractions.Models;

namespace EchoSphere.Users.Abstractions;

public interface IFriendService
{
	ValueTask<IReadOnlyList<UserId>> GetFriends(UserId userId, CancellationToken cancellationToken);

	ValueTask SendFriendInvite(UserId fromUserId, UserId toUserId, CancellationToken cancellationToken);

	ValueTask<IReadOnlyList<UserId>> GetFriendInvites(UserId userId, CancellationToken cancellationToken);

	ValueTask AcceptFriendInvite(UserId fromUserId, UserId toUserId, CancellationToken cancellationToken);

	ValueTask RejectFriendInvite(UserId fromUserId, UserId toUserId, CancellationToken cancellationToken);
}