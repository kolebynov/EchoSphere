using EchoSphere.Domain.Abstractions.Models;
using EchoSphere.Posts.Abstractions.IntegrationEvents;
using EchoSphere.Users.Abstractions;

namespace EchoSphere.RealtimeNotifications.Api.EventUsersProviders;

internal sealed class PostPublishedEventUsersProvider : IEventUsersProvider<PostPublished>
{
	private readonly IFollowService _followService;

	public PostPublishedEventUsersProvider(IFollowService followService)
	{
		_followService = followService;
	}

	public async ValueTask<IEnumerable<UserId>> GetEventUsers(PostPublished @event, CancellationToken cancellationToken)
	{
		var followers = await _followService.GetFollowers(@event.UserId, cancellationToken);
		return followers.IfNone([]);
	}
}