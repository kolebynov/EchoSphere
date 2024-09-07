namespace EchoSphere.ApiGateway.Contracts;

public sealed class ChatInfoDtoV1
{
	public required Guid Id { get; init; }

	public required IReadOnlyList<Guid> Participants { get; init; }
}