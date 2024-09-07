namespace EchoSphere.ApiGateway.Contracts;

public sealed class CreateChatRequestV1
{
	public required IReadOnlyList<Guid> Participants { get; init; }
}