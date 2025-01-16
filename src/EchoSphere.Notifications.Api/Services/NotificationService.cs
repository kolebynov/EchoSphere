using EchoSphere.Domain.Abstractions;
using EchoSphere.Notifications.Abstractions;
using EchoSphere.Notifications.Abstractions.Models;
using EchoSphere.Notifications.Api.Abstractions;
using EchoSphere.Notifications.Api.Data.Models;
using LinqToDB;
using LinqToDB.Data;

namespace EchoSphere.Notifications.Api.Services;

internal sealed class NotificationService : INotificationService, INotificationStorage
{
	private readonly ITable<NotificationDb> _notificationsTable;
	private readonly ICurrentUserAccessor _currentUserAccessor;

	public NotificationService(IDataContext dataConnection, ICurrentUserAccessor currentUserAccessor)
	{
		_currentUserAccessor = currentUserAccessor;
		_notificationsTable = dataConnection.GetTable<NotificationDb>();
	}

	public async Task<IReadOnlyList<Notification>> GetCurrentUserNotifications(CancellationToken cancellationToken) =>
		await _notificationsTable
			.Where(x => x.UserId == _currentUserAccessor.CurrentUserId)
			.Select(x => new Notification
			{
				Id = x.Id,
				Text = x.Text,
				IsRead = x.IsRead,
			})
			.ToArrayAsync(cancellationToken);

	public Task MarkAsRead(IReadOnlyList<NotificationId> notificationIds, CancellationToken cancellationToken) =>
		_notificationsTable.UpdateAsync(
			x => x.UserId == _currentUserAccessor.CurrentUserId && notificationIds.Contains(x.Id),
			x => new NotificationDb { IsRead = true },
			cancellationToken);

	public Task DeleteCurrentUserNotifications(CancellationToken cancellationToken) =>
		_notificationsTable.DeleteAsync(x => x.UserId == _currentUserAccessor.CurrentUserId, cancellationToken);

	public Task AddNotifications(IEnumerable<AddNotificationData> notifications, CancellationToken cancellationToken)
	{
		var notificationsToAdd = notifications.Select(x => new NotificationDb
		{
			UserId = x.UserId,
			Text = x.NotificationBody,
			IsRead = false,
		});

		return _notificationsTable.BulkCopyAsync(notificationsToAdd, cancellationToken);
	}
}