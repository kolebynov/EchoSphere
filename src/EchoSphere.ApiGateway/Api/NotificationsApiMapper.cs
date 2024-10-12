using EchoSphere.ApiGateway.Contracts;
using EchoSphere.Notifications.Abstractions;
using EchoSphere.Notifications.Abstractions.Models;
using Microsoft.AspNetCore.Mvc;

namespace EchoSphere.ApiGateway.Api;

public static class NotificationsApiMapper
{
	public static IEndpointRouteBuilder MapNotificationsApi(this IEndpointRouteBuilder routeBuilder)
	{
		var notificationsApi = routeBuilder.MapGroup("notifications");
		notificationsApi.MapGet("/", GetNotifications);
		notificationsApi.MapPost("/markAsRead", MarkAsRead);
		notificationsApi.MapPost("/deleteAll", DeleteNotifications);

		return routeBuilder;
	}

	private static async Task<IEnumerable<NotificationDtoV1>> GetNotifications(
		INotificationService notificationService, CancellationToken cancellationToken)
	{
		var notifications = await notificationService.GetCurrentUserNotifications(cancellationToken);
		return notifications
			.Select(x => new NotificationDtoV1
			{
				Id = x.Id.Value,
				Text = x.Text,
				IsRead = x.IsRead,
			});
	}

	private static Task MarkAsRead(
		INotificationService notificationService, [FromBody] IReadOnlyCollection<long> notificationIds,
		CancellationToken cancellationToken) =>
		notificationService.MarkAsRead(
			notificationIds.Select(x => new NotificationId(x)).ToArray(), cancellationToken);

	private static Task DeleteNotifications(
		INotificationService notificationService, CancellationToken cancellationToken) =>
		notificationService.DeleteCurrentUserNotifications(cancellationToken);
}