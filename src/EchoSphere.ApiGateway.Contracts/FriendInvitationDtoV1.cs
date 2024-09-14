namespace EchoSphere.ApiGateway.Contracts;

public sealed class FriendInvitationDtoV1
{
	public required Guid Id { get; init; }

	public required Guid FromUserId { get; init; }
}