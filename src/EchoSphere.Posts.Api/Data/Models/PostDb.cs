using EchoSphere.Posts.Abstractions.Models;
using EchoSphere.Users.Abstractions.Models;

namespace EchoSphere.Posts.Api.Data.Models;

public sealed class PostDb
{
	public required PostId Id { get; init; }

	public required UserId UserId { get; init; }

	public required string Title { get; init; }

	public required string Body { get; init; }
}