namespace EchoSphere.ApiGateway.Contracts;

public sealed class ChatMessageDtoV1
{
	public required long Id { get; init; }

	public required DateTimeOffset Timestamp { get; init; }

	public required Guid SenderId { get; init; }

	public required string Text { get; init; }
}