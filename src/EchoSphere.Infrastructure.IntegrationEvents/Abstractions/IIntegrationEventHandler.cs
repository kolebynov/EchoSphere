namespace EchoSphere.Infrastructure.IntegrationEvents.Abstractions;

public interface IIntegrationEventHandler<TEvent>
	where TEvent : IIntegrationEvent
{
	ValueTask Handle(TEvent @event, CancellationToken cancellationToken);
}