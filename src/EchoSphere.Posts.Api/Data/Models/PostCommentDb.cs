using EchoSphere.Domain.Abstractions.Models;
using EchoSphere.Posts.Abstractions.Models;

namespace EchoSphere.Posts.Api.Data.Models;

internal sealed class PostCommentDb
{
	public required PostCommentId Id { get; init; }

	public required string Text { get; init; }

	public required UserId UserId { get; init; }

	public required PostId PostId { get; init; }
}