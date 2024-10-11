using System.Linq.Expressions;
using System.Reflection;
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
	private readonly Dictionary<Type, Func<IEventHandler>> _eventHandlerProviders = new();

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

		var messagePoller = new MessagesPoller<Null, SerializedIntegrationEvent>(_consumer, _integrationEventsSettings.BatchSize);

		while (!stopCancellationToken.IsCancellationRequested)
		{
			var consumeResults = messagePoller.PollBatch(stopCancellationToken);
			using var scope = scopeServiceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope();

			foreach (var consumeResult in consumeResults)
			{
				var integrationEvent = _eventSerializer.Deserialize(consumeResult.Message.Value);
				await GetEventHandler(integrationEvent.GetType())
					.Handle(integrationEvent, scope.ServiceProvider, stopCancellationToken);

				_consumer.StoreOffset(consumeResult);
			}
		}
	}

	private IEventHandler GetEventHandler(Type integrationEventType)
	{
		if (_eventHandlerProviders.TryGetValue(integrationEventType, out var handlerProvider))
		{
			return handlerProvider();
		}

		var eventHandlerType = typeof(EventHandler<>).MakeGenericType(integrationEventType);
		var instanceField = eventHandlerType.GetField(
			nameof(EventHandler<IIntegrationEvent>.Instance), BindingFlags.Public | BindingFlags.Static)!;
		var handlerProviderExpression = Expression.Field(null, instanceField);
		_eventHandlerProviders[integrationEventType] = handlerProvider =
			Expression.Lambda<Func<IEventHandler>>(handlerProviderExpression).Compile();

		return handlerProvider();
	}

	private interface IEventHandler
	{
		ValueTask Handle(IIntegrationEvent integrationEvent, IServiceProvider serviceProvider,
			CancellationToken cancellationToken);
	}

	private sealed class EventHandler<TEvent> : IEventHandler
		where TEvent : IIntegrationEvent
	{
		public static readonly IEventHandler Instance = new EventHandler<TEvent>();

		public async ValueTask Handle(IIntegrationEvent integrationEvent, IServiceProvider serviceProvider,
			CancellationToken cancellationToken)
		{
			var handlers = serviceProvider.GetServices<IIntegrationEventHandler<TEvent>>();
			var @event = (TEvent)integrationEvent;

			foreach (var handler in handlers)
			{
				await handler.Handle(@event, cancellationToken);
			}
		}
	}
}