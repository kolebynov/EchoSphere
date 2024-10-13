using EchoSphere.Domain.Abstractions.Models;
using EchoSphere.Infrastructure.IntegrationEvents;

namespace EchoSphere.Posts.Abstractions.IntegrationEvents;

public sealed class PostLiked : IIntegrationEvent
{
	public required PostId PostId { get; init; }

	public required UserId PostAuthorId { get; init; }

	public required UserId UserId { get; init; }
}