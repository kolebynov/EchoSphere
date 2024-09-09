namespace EchoSphere.ApiGateway.Contracts;

public sealed class PublishPostRequestV1
{
	public required string Title { get; init; }

	public required string Body { get; init; }
}