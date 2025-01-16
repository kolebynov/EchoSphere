using EchoSphere.Infrastructure.IntegrationEvents;
using EchoSphere.Infrastructure.IntegrationEvents.Abstractions;
using EchoSphere.Notifications.Api.Abstractions;

namespace EchoSphere.Notifications.Api.Services;

internal sealed class EventToNotificationsHandler<TEvent, TConverter> : IIntegrationEventHandler<TEvent>
	where TEvent : IIntegrationEvent
	where TConverter : IEventToNotificationsConverter<TEvent>
{
	private readonly TConverter _converter;
	private readonly INotificationStorage _notificationStorage;

	public EventToNotificationsHandler(TConverter converter, INotificationStorage notificationStorage)
	{
		_converter = converter;
		_notificationStorage = notificationStorage;
	}

	public async ValueTask Handle(TEvent @event, CancellationToken cancellationToken)
	{
		var notifications = await _converter.Convert(@event, cancellationToken);
		await _notificationStorage.AddNotifications(notifications, cancellationToken);
	}
}