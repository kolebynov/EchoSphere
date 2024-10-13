using EchoSphere.Domain.Abstractions;
using EchoSphere.Infrastructure.IntegrationEvents.Abstractions;
using EchoSphere.Notifications.Abstractions;
using EchoSphere.Notifications.Abstractions.Models;
using EchoSphere.Notifications.Api.Data.Models;
using EchoSphere.Posts.Abstractions.IntegrationEvents;
using EchoSphere.Users.Abstractions;
using LinqToDB;
using LinqToDB.Data;

namespace EchoSphere.Notifications.Api.Services;

internal sealed class NotificationService : INotificationService, IIntegrationEventHandler<PostPublished>,
	IIntegrationEventHandler<PostLiked>, IIntegrationEventHandler<PostCommentAdded>
{
	private readonly IDataContext _dataConnection;
	private readonly ITable<NotificationDb> _notificationsTable;
	private readonly ICurrentUserAccessor _currentUserAccessor;
	private readonly IFollowService _followService;

	public NotificationService(IDataContext dataConnection, ICurrentUserAccessor currentUserAccessor, IFollowService followService)
	{
		_dataConnection = dataConnection;
		_currentUserAccessor = currentUserAccessor;
		_followService = followService;
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

	public async ValueTask Handle(PostPublished @event, CancellationToken cancellationToken)
	{
		var followers = (await _followService.GetFollowers(@event.UserId, cancellationToken)).IfNone([]);
		if (followers.Count == 0)
		{
			return;
		}

		var notificationText = $"Hey, {@event.UserId.Value} has just published post {@event.PostId.Value}";
		await _notificationsTable.BulkCopyAsync(
			followers.Select(f => new NotificationDb
			{
				Text = notificationText,
				IsRead = false,
				UserId = f,
			}), cancellationToken);
	}

	public async ValueTask Handle(PostLiked @event, CancellationToken cancellationToken)
	{
		if (@event.UserId == @event.PostAuthorId)
		{
			return;
		}

		await _dataConnection.InsertAsync(
			new NotificationDb
			{
				UserId = @event.PostAuthorId,
				Text = $"User {@event.UserId.Value} has liked your post {@event.PostId.Value}",
			}, token: cancellationToken);
	}

	public async ValueTask Handle(PostCommentAdded @event, CancellationToken cancellationToken)
	{
		if (@event.UserId == @event.PostAuthorId)
		{
			return;
		}

		await _dataConnection.InsertAsync(
			new NotificationDb
			{
				UserId = @event.PostAuthorId,
				Text = $"User {@event.UserId.Value} has added comment to your post {@event.PostId.Value}",
			}, token: cancellationToken);
	}
}