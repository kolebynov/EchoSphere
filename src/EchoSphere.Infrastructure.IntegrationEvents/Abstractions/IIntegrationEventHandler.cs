namespace EchoSphere.Infrastructure.IntegrationEvents.Abstractions;

public interface IIntegrationEventHandler<in TEvent>
	where TEvent : IIntegrationEvent
{
	ValueTask Handle(TEvent @event, CancellationToken cancellationToken);
}