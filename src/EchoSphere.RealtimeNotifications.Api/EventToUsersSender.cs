using EchoSphere.Domain.Abstractions.Extensions;
using EchoSphere.Infrastructure.IntegrationEvents;
using EchoSphere.Infrastructure.IntegrationEvents.Abstractions;
using EchoSphere.RealtimeNotifications.Api.EventUsersProviders;
using Microsoft.AspNetCore.SignalR;

namespace EchoSphere.RealtimeNotifications.Api;

internal class EventToUsersSender<TEvent, TProvider> : IIntegrationEventHandler<TEvent>
	where TEvent : IIntegrationEvent
	where TProvider : IEventUsersProvider<TEvent>
{
	private static readonly string EventName = typeof(TEvent).FullName!;

	private readonly TProvider _usersProvider;
	private readonly IHubContext<NotificationsHub> _hubContext;

	public EventToUsersSender(TProvider usersProvider, IHubContext<NotificationsHub> hubContext)
	{
		_usersProvider = usersProvider;
		_hubContext = hubContext;
	}

	public async ValueTask Handle(TEvent @event, CancellationToken cancellationToken)
	{
		var users = (await _usersProvider.GetEventUsers(@event, cancellationToken))
			.Select(x => x.ToInnerString());
		await _hubContext.Clients.Users(users).SendAsync(EventName, @event, cancellationToken);
	}
}