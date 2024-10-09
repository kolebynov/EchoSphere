namespace EchoSphere.Infrastructure.IntegrationEvents.Abstractions;

public interface IIntegrationEventService
{
	Task PublishEvent<T>(T @event, CancellationToken cancellationToken)
		where T : class, IIntegrationEvent;
}