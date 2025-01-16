using EchoSphere.Infrastructure.IntegrationEvents;

namespace EchoSphere.Notifications.Api.Abstractions;

internal interface IEventToNotificationsConverter<TEvent>
	where TEvent : IIntegrationEvent
{
	ValueTask<IEnumerable<AddNotificationData>> Convert(TEvent integrationEvent, CancellationToken cancellationToken);
}