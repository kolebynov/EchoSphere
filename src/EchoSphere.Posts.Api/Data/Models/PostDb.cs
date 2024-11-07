using EchoSphere.Domain.Abstractions.Models;

namespace EchoSphere.Posts.Api.Data.Models;

public sealed class PostDb
{
	public required PostId Id { get; init; }

	public required DateTimeOffset PostedOn { get; init; }

	public required UserId AuthorId { get; init; }

	public required string Body { get; init; }
}