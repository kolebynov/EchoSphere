using Refit;

namespace EchoSphere.ApiGateway.Client;

public interface IFriendClient
{
	[Get("/users/{userId}/friends")]
	Task<IReadOnlyList<Guid>> GetFriends(string userId, CancellationToken cancellationToken);

	[Post("/users/{toUserId}/sendFriendInvite")]
	Task SendFriendInvite(Guid toUserId, CancellationToken cancellationToken);

	[Get("/users/me/friendInvites")]
	Task<IReadOnlyList<Guid>> GetFriendInvites(CancellationToken cancellationToken);

	[Post("/users/me/friendInvites/{fromUserId}/accept")]
	Task AcceptFriendInvite(Guid fromUserId, CancellationToken cancellationToken);

	[Post("/users/me/friendInvites/{fromUserId}/reject")]
	Task RejectFriendInvite(Guid fromUserId, CancellationToken cancellationToken);
}