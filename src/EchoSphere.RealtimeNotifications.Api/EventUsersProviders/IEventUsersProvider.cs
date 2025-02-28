using EchoSphere.Domain.Abstractions.Models;
using EchoSphere.Infrastructure.IntegrationEvents;

namespace EchoSphere.RealtimeNotifications.Api.EventUsersProviders;

internal interface IEventUsersProvider<TEvent>
	where TEvent : IIntegrationEvent
{
	ValueTask<IEnumerable<UserId>> GetEventUsers(TEvent @event, CancellationToken cancellationToken);
}