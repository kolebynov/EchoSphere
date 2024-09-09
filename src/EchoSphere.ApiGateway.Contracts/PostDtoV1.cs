namespace EchoSphere.ApiGateway.Contracts;

public sealed class PostDtoV1
{
	public required Guid Id { get; init; }

	public required string Title { get; init; }

	public required string Body { get; init; }
}