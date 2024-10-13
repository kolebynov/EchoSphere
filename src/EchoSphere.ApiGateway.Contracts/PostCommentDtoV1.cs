namespace EchoSphere.ApiGateway.Contracts;

public sealed class PostCommentDtoV1
{
	public required Guid Id { get; init; }

	public required string Text { get; init; }

	public required Guid UserId { get; init; }
}