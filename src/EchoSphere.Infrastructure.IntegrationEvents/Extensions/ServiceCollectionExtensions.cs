using System.Reflection;
using EchoSphere.Infrastructure.Db.Settings;
using EchoSphere.Infrastructure.IntegrationEvents.Abstractions;
using EchoSphere.Infrastructure.IntegrationEvents.Data;
using EchoSphere.Infrastructure.IntegrationEvents.Data.Models;
using EchoSphere.Infrastructure.IntegrationEvents.Internal;
using LinqToDB.Mapping;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EchoSphere.Infrastructure.IntegrationEvents.Extensions;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddIntegrationEvents(this IServiceCollection services)
	{
		services.AddOptions<DbSettings>()
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

		services.AddHostedService<IntegrationEventHostedService>();

		services.TryAddScoped<IIntegrationEventService, IntegrationEventService>();

		services.TryAddSingleton<IEventSerializer, EventSerializer>();

		return services;
	}
}