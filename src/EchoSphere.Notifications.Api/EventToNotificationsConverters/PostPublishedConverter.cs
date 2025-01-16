using EchoSphere.Notifications.Api.Abstractions;
using EchoSphere.Posts.Abstractions.IntegrationEvents;
using EchoSphere.Users.Abstractions;

namespace EchoSphere.Notifications.Api.EventToNotificationsConverters;

internal sealed class PostPublishedConverter : IEventToNotificationsConverter<PostPublished>
{
	private readonly IFollowService _followService;

	public PostPublishedConverter(IFollowService followService)
	{
		_followService = followService;
	}

	public async ValueTask<IEnumerable<AddNotificationData>> Convert(
		PostPublished integrationEvent, CancellationToken cancellationToken)
	{
		var followers = (await _followService.GetFollowers(integrationEvent.UserId, cancellationToken)).IfNone([]);
		if (followers.Count == 0)
		{
			return [];
		}

		var notificationText = $"Hey, {integrationEvent.UserId.Value} has just published post {integrationEvent.PostId.Value}";
		return followers.Select(f => new AddNotificationData(f, notificationText));
	}
}