using EchoSphere.ApiGateway.Contracts;
using Refit;

namespace EchoSphere.ApiGateway.Client;

public interface INotificationClient
{
	[Get("/notifications")]
	Task<IReadOnlyList<NotificationDtoV1>> GetCurrentUserNotifications(CancellationToken cancellationToken);

	[Post("/notifications/markAsRead")]
	Task MarkAsRead(
		[Body] IReadOnlyCollection<long> notificationIds, CancellationToken cancellationToken);

	[Post("/notifications/deleteAll")]
	Task DeleteCurrentUserNotifications(CancellationToken cancellationToken);
}