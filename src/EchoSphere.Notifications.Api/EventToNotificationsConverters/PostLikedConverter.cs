using EchoSphere.Notifications.Api.Abstractions;
using EchoSphere.Posts.Abstractions.IntegrationEvents;

namespace EchoSphere.Notifications.Api.EventToNotificationsConverters;

internal sealed class PostLikedConverter : IEventToNotificationsConverter<PostLiked>
{
	public ValueTask<IEnumerable<AddNotificationData>> Convert(
		PostLiked integrationEvent, CancellationToken cancellationToken)
	{
		IEnumerable<AddNotificationData> result = integrationEvent.UserId != integrationEvent.PostAuthorId
			?
			[
				new AddNotificationData(
					integrationEvent.PostAuthorId,
					$"User {integrationEvent.UserId.Value} has liked your post {integrationEvent.PostId.Value}")
			]
			: [];

		return ValueTask.FromResult(result);
	}
}