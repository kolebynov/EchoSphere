using EchoSphere.Domain.Abstractions.Models;
using EchoSphere.Infrastructure.IntegrationEvents;
using EchoSphere.Posts.Abstractions.Models;

namespace EchoSphere.Posts.Abstractions.IntegrationEvents;

public sealed class PostCommentAdded : IIntegrationEvent
{
	public required PostCommentId PostCommentId { get; init; }

	public required PostId PostId { get; init; }

	public required UserId PostAuthorId { get; init; }

	public required UserId UserId { get; init; }
}