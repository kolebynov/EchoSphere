using EchoSphere.Domain.Abstractions.Models;

namespace EchoSphere.Posts.Abstractions.Models;

public sealed class Post
{
	public required PostId Id { get; init; }

	public required string Title { get; init; }

	public required string Body { get; init; }

	public required bool LikedByCurrentUser { get; init; }

	public required int LikesCount { get; init; }
}