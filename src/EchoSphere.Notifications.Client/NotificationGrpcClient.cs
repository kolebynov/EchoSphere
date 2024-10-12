using EchoSphere.GrpcClientShared;
using EchoSphere.GrpcShared.Extensions;
using EchoSphere.Notifications.Abstractions;
using EchoSphere.Notifications.Abstractions.Models;
using EchoSphere.Notifications.Grpc;

namespace EchoSphere.Notifications.Client;

internal sealed class NotificationGrpcClient : INotificationService
{
	private readonly GrpcCallExecutor<NotificationService.NotificationServiceClient> _grpcExecutor;

	public NotificationGrpcClient(GrpcCallExecutor<NotificationService.NotificationServiceClient> grpcExecutor)
	{
		_grpcExecutor = grpcExecutor;
	}

	public Task<IReadOnlyList<Notification>> GetCurrentUserNotifications(CancellationToken cancellationToken) =>
		_grpcExecutor.ExecuteAsync<IReadOnlyList<Notification>>(async client =>
		{
			var notifications = await client.GetCurrentUserNotificationsAsync(
				GrpcExtensions.EmptyInstance, cancellationToken: cancellationToken);
			return notifications.Notifications
				.Select(x => new Notification
				{
					Id = new NotificationId(x.Id),
					Text = x.Text,
					IsRead = x.IsRead,
				})
				.ToArray();
		});

	public Task MarkAsRead(IReadOnlyList<NotificationId> notificationIds, CancellationToken cancellationToken) =>
		_grpcExecutor.ExecuteAsync(async client =>
		{
			await client.MarkAsReadAsync(
				new NotificationIdsDto
				{
					Ids = { notificationIds.Select(x => x.Value) },
				},
				cancellationToken: cancellationToken);
			return Unit.Default;
		});

	public Task DeleteCurrentUserNotifications(CancellationToken cancellationToken) =>
		_grpcExecutor.ExecuteAsync(async client =>
		{
			await client.DeleteCurrentUserNotificationsAsync(
				GrpcExtensions.EmptyInstance, cancellationToken: cancellationToken);
			return Unit.Default;
		});
}