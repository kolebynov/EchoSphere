using EchoSphere.Domain.Abstractions.Models;
using EchoSphere.Posts.Abstractions.IntegrationEvents;

namespace EchoSphere.RealtimeNotifications.Api.EventUsersProviders;

internal sealed class PostCommentEventUsersProvider : IEventUsersProvider<PostCommentAdded>
{
	public ValueTask<IEnumerable<UserId>> GetEventUsers(PostCommentAdded @event, CancellationToken cancellationToken) =>
		ValueTask.FromResult<IEnumerable<UserId>>([@event.PostAuthorId]);
}