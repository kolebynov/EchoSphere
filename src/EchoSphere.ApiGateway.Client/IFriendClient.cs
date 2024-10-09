using EchoSphere.ApiGateway.Contracts;
using Refit;

namespace EchoSphere.ApiGateway.Client;

public interface IFriendClient
{
	[Get("/users/{userId}/friends")]
	Task<IReadOnlyList<Guid>> GetFriends(string userId, CancellationToken cancellationToken);

	[Post("/users/{toUserId}/sendFriendInvite")]
	Task SendFriendInvite(Guid toUserId, CancellationToken cancellationToken);

	[Get("/friendInvites")]
	Task<IReadOnlyList<FriendInvitationDtoV1>> GetFriendInvites(CancellationToken cancellationToken);

	[Post("/friendInvites/{invitationId}/accept")]
	Task AcceptFriendInvite(Guid invitationId, CancellationToken cancellationToken);

	[Post("/friendInvites/{invitationId}/reject")]
	Task RejectFriendInvite(Guid invitationId, CancellationToken cancellationToken);
}