namespace EchoSphere.Notifications.Api.Abstractions;

internal interface INotificationStorage
{
	Task AddNotifications(IEnumerable<AddNotificationData> notifications, CancellationToken cancellationToken);
}