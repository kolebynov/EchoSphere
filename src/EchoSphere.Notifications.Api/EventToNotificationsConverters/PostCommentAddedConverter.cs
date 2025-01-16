using EchoSphere.Notifications.Api.Abstractions;
using EchoSphere.Posts.Abstractions.IntegrationEvents;

namespace EchoSphere.Notifications.Api.EventToNotificationsConverters;

internal sealed class PostCommentAddedConverter : IEventToNotificationsConverter<PostCommentAdded>
{
	public ValueTask<IEnumerable<AddNotificationData>> Convert(
		PostCommentAdded integrationEvent, CancellationToken cancellationToken)
	{
		IEnumerable<AddNotificationData> result = integrationEvent.UserId != integrationEvent.PostAuthorId
			?
			[
				new AddNotificationData(integrationEvent.PostAuthorId,
					$"User {integrationEvent.UserId.Value} has added comment to your post {integrationEvent.PostId.Value}")
			]
			: [];

		return ValueTask.FromResult(result);
	}
}