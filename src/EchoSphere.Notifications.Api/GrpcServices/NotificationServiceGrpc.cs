using EchoSphere.GrpcShared.Extensions;
using EchoSphere.Notifications.Abstractions;
using EchoSphere.Notifications.Abstractions.Models;
using EchoSphere.Notifications.Grpc;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using LanguageExt;
using Microsoft.AspNetCore.Authorization;

namespace EchoSphere.Notifications.Api.GrpcServices;

[Authorize]
public sealed class NotificationServiceGrpc : NotificationService.NotificationServiceBase
{
	private readonly INotificationService _notificationService;

	public NotificationServiceGrpc(INotificationService notificationService)
	{
		_notificationService = notificationService;
	}

	public override Task<NotificationsDto> GetCurrentUserNotifications(Empty request, ServerCallContext context) =>
		_notificationService.GetCurrentUserNotifications(context.CancellationToken)
			.Map(notifications => new NotificationsDto
			{
				Notifications =
				{
					notifications.Select(x => new NotificationDto
					{
						Id = x.Id.Value,
						Text = x.Text,
						IsRead = x.IsRead,
					}),
				},
			});

	public override async Task<Empty> MarkAsRead(NotificationIdsDto request, ServerCallContext context)
	{
		await _notificationService
			.MarkAsRead(request.Ids.Select(x => new NotificationId(x)).ToArray(), context.CancellationToken);
		return GrpcExtensions.EmptyInstance;
	}

	public override async Task<Empty> DeleteCurrentUserNotifications(Empty request, ServerCallContext context)
	{
		await _notificationService.DeleteCurrentUserNotifications(context.CancellationToken);
		return GrpcExtensions.EmptyInstance;
	}
}