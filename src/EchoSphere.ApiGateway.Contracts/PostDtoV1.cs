namespace EchoSphere.ApiGateway.Contracts;

public sealed class PostDtoV1
{
	public required Guid Id { get; init; }

	public required DateTimeOffset PostedOn { get; init; }

	public required Guid AuthorId { get; init; }

	public required string Body { get; init; }

	public required bool LikedByCurrentUser { get; init; }

	public required int LikesCount { get; init; }

	public required int CommentsCount { get; init; }
}