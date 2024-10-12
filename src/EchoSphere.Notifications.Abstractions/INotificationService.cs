using EchoSphere.Notifications.Abstractions.Models;

namespace EchoSphere.Notifications.Abstractions;

public interface INotificationService
{
	Task<IReadOnlyList<Notification>> GetCurrentUserNotifications(CancellationToken cancellationToken);

	Task MarkAsRead(IReadOnlyList<NotificationId> notificationIds, CancellationToken cancellationToken);

	Task DeleteCurrentUserNotifications(CancellationToken cancellationToken);
}