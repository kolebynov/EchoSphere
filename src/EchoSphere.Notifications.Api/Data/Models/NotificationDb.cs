using EchoSphere.Domain.Abstractions.Models;
using EchoSphere.Notifications.Abstractions.Models;

namespace EchoSphere.Notifications.Api.Data.Models;

internal sealed class NotificationDb
{
	public NotificationId Id { get; init; }

	public string Text { get; init; } = null!;

	public bool IsRead { get; init; }

	public UserId UserId { get; init; }
}