namespace EchoSphere.Notifications.Abstractions.Models;

public sealed class Notification
{
	public required NotificationId Id { get; init; }

	public required string Text { get; init; }

	public required bool IsRead { get; init; }
}