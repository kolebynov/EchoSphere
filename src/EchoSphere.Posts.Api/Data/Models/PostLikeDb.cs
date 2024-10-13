using EchoSphere.Domain.Abstractions.Models;

namespace EchoSphere.Posts.Api.Data.Models;

internal sealed class PostLikeDb
{
	public UserId UserId { get; init; }

	public PostId PostId { get; init; }
}