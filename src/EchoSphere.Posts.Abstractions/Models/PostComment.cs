using EchoSphere.Domain.Abstractions.Models;

namespace EchoSphere.Posts.Abstractions.Models;

public sealed class PostComment
{
	public required PostCommentId Id { get; init; }

	public required string Text { get; init; }

	public required UserId UserId { get; init; }
}