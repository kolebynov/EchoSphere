using System.Diagnostics;
using System.Linq.Expressions;
using Confluent.Kafka;
using EchoSphere.Infrastructure.Hosting;
using EchoSphere.Infrastructure.IntegrationEvents.Abstractions;
using EchoSphere.Infrastructure.IntegrationEvents.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EchoSphere.Infrastructure.IntegrationEvents.Internal;

internal sealed class ConsumeIntegrationEventHostedService : BaseHostedService
{
	private readonly IConsumer<Null, SerializedIntegrationEvent> _consumer;
	private readonly ILogger<ConsumeIntegrationEventHostedService> _logger;
	private readonly IntegrationEventsSettings _integrationEventsSettings;
	private readonly IEventSerializer _eventSerializer;
	private readonly Dictionary<Type, Func<IServiceProvider, IEventHandler>> _eventHandlerFactories = new();

	public ConsumeIntegrationEventHostedService(
		IServiceScopeFactory serviceScopeFactory, IConsumer<Null, SerializedIntegrationEvent> consumer,
		ILogger<ConsumeIntegrationEventHostedService> logger, IOptions<IntegrationEventsSettings> integrationEventsSettings,
		IEventSerializer eventSerializer)
		: base(serviceScopeFactory)
	{
		_consumer = consumer;
		_logger = logger;
		_eventSerializer = eventSerializer;
		_integrationEventsSettings = integrationEventsSettings.Value;
	}

	protected override async Task RunAsync(IServiceProvider scopeServiceProvider, CancellationToken stopCancellationToken)
	{
		_consumer.Subscribe(_integrationEventsSettings.ListenTopicNames);

		var batchConsumer = new BatchKafkaConsumer<Null, SerializedIntegrationEvent>(_consumer, _integrationEventsSettings.BatchSize);
		var eventHandlers = new Dictionary<Type, IEventHandler>();

		while (!stopCancellationToken.IsCancellationRequested)
		{
			var consumeResults = batchConsumer.ConsumeBatch(stopCancellationToken);
			_logger.LogInformation("Integration events consumed. [Count: {Count}]", consumeResults.Count);

			using var scope = scopeServiceProvider.CreateScope();
			eventHandlers.Clear();

			foreach (var consumeResult in consumeResults)
			{
				KeyValuePair<string, object?>[] activityTags = [new("IntegrationEventType", consumeResult.Message.Value.TypeName)];
				using (var _ =
				       ActivitySources.Source.StartActivity(ActivityKind.Consumer, name: "Handle integration event", tags: activityTags))
				{
					var integrationEvent = _eventSerializer.Deserialize(consumeResult.Message.Value);
					await GetEventHandler(eventHandlers, integrationEvent.GetType(), scope.ServiceProvider)
						.Handle(integrationEvent, stopCancellationToken);
				}

				_consumer.StoreOffset(consumeResult);
			}
		}
	}

	private IEventHandler GetEventHandler(Dictionary<Type, IEventHandler> eventHandlers, Type integrationEventType,
		IServiceProvider serviceProvider)
	{
		if (eventHandlers.TryGetValue(integrationEventType, out var handler))
		{
			return handler;
		}

		if (!_eventHandlerFactories.TryGetValue(integrationEventType, out var handlerFactory))
		{
			_eventHandlerFactories[integrationEventType] = handlerFactory = CreateEventHandlerFactory(integrationEventType);
		}

		return eventHandlers[integrationEventType] = handlerFactory(serviceProvider);
	}

	private static Func<IServiceProvider, IEventHandler> CreateEventHandlerFactory(Type integrationEventType)
	{
		var serviceProviderParameter = Expression.Parameter(typeof(IServiceProvider), "serviceProvider");
		var eventHandlerCtor = typeof(EventHandler<>).MakeGenericType(integrationEventType).GetConstructors()[0];
		var factoryExpr = Expression.Lambda<Func<IServiceProvider, IEventHandler>>(
			Expression.Convert(
				Expression.New(eventHandlerCtor, serviceProviderParameter),
				typeof(IEventHandler)),
			serviceProviderParameter);

		return factoryExpr.Compile();
	}

	private interface IEventHandler
	{
		ValueTask Handle(IIntegrationEvent integrationEvent, CancellationToken cancellationToken);
	}

	private sealed class EventHandler<TEvent> : IEventHandler
		where TEvent : IIntegrationEvent
	{
		private readonly IIntegrationEventHandler<TEvent>[] _handlers;

		public EventHandler(IServiceProvider serviceProvider)
		{
			_handlers = serviceProvider.GetServices<IIntegrationEventHandler<TEvent>>().ToArray();
		}

		public async ValueTask Handle(IIntegrationEvent integrationEvent, CancellationToken cancellationToken)
		{
			var @event = (TEvent)integrationEvent;

			foreach (var handler in _handlers)
			{
				await handler.Handle(@event, cancellationToken);
			}
		}
	}
}