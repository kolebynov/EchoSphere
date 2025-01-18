using System.Reflection;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using EchoSphere.Infrastructure.Db.Settings;
using EchoSphere.Infrastructure.Hosting.Extensions;
using EchoSphere.Infrastructure.IntegrationEvents.Abstractions;
using EchoSphere.Infrastructure.IntegrationEvents.Data;
using EchoSphere.Infrastructure.IntegrationEvents.Data.Models;
using EchoSphere.Infrastructure.IntegrationEvents.Internal;
using EchoSphere.Infrastructure.IntegrationEvents.Settings;
using LinqToDB.Mapping;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace EchoSphere.Infrastructure.IntegrationEvents.Extensions;

public static class ServiceCollectionExtensions
{
	private const string IntegrationEventsSectionName = "IntegrationEvents";

	public static IHostApplicationBuilder AddIntegrationEvents(this IHostApplicationBuilder builder)
	{
		var integrationEventsSettings = new IntegrationEventsSettings();
		var configurationSection = builder.Configuration.GetSection(IntegrationEventsSectionName);
		configurationSection.Bind(integrationEventsSettings);

		builder.Services.Configure<IntegrationEventsSettings>(configurationSection);

		if (!integrationEventsSettings.DisableProducer)
		{
			AddProducer(builder);
		}

		if (!integrationEventsSettings.DisableConsumer)
		{
			var serviceName = !string.IsNullOrEmpty(integrationEventsSettings.ServiceName)
				? integrationEventsSettings.ServiceName
				: throw new InvalidOperationException("Service name must be specified if consumer is enabled.");
			AddConsumer(builder, serviceName);
		}

		builder.Services.TryAddSingleton<IEventSerializer, EventSerializer>();

		return builder;
	}

	private static void AddProducer(IHostApplicationBuilder builder)
	{
		builder.AddKafkaProducer<Null, SerializedIntegrationEvent>(
			"kafka",
			settings =>
			{
				settings.Config.Acks = Acks.All;
			},
			producerBuilder => producerBuilder.SetValueSerializer(new IntegrationEventKafkaSerializer()));

		builder.Services.AddHostedService<ProduceIntegrationEventHostedService>();

		builder.Services.AddScopedAsyncInitializer((sp, _) =>
		{
			var connectionString = sp.GetRequiredService<IConfiguration>().GetConnectionString("kafka");
			var config = new AdminClientConfig
			{
				BootstrapServers = connectionString,
			};

			var topicName = sp.GetRequiredService<IOptions<IntegrationEventsSettings>>().Value.TopicName;
			var adminClient = new AdminClientBuilder(config).Build();
			return adminClient.CreateTopicsAsync([
				new TopicSpecification { Name = topicName, NumPartitions = 1, ReplicationFactor = 1 }
			]);
		});

		builder.Services.AddOptions<DbSettings>()
			.PostConfigure(dbSettings =>
			{
				dbSettings.MigrationAssemblies = [..dbSettings.MigrationAssemblies, Assembly.GetExecutingAssembly()];

				var fluentMappingBuilder = new FluentMappingBuilder(dbSettings.MappingSchema);
				fluentMappingBuilder.Entity<IntegrationEventDb>()
					.HasTableName(DataConstants.EventsTableName)
					.HasPrimaryKey(x => x.Id)
					.HasIdentity(x => x.Id);

				fluentMappingBuilder.Build();
			});

		builder.Services.TryAddScoped<IIntegrationEventService, IntegrationEventService>();
	}

	private static void AddConsumer(IHostApplicationBuilder builder, string serviceName)
	{
		builder.AddKafkaConsumer<Null, SerializedIntegrationEvent>(
			"kafka",
			settings =>
			{
				settings.Config.GroupId = serviceName;
				settings.Config.EnableAutoOffsetStore = false;
			},
			consumerBuilder => consumerBuilder.SetValueDeserializer(new IntegrationEventKafkaSerializer()));

		builder.Services.AddHostedService<ConsumeIntegrationEventHostedService>();
	}
}