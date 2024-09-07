namespace EchoSphere.ApiGateway.Contracts;

public sealed class SendMessageRequestV1
{
	public required string Text { get; init; }
}