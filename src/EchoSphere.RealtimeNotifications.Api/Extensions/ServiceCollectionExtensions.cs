using EchoSphere.Infrastructure.IntegrationEvents;
using EchoSphere.Infrastructure.IntegrationEvents.Abstractions;
using EchoSphere.RealtimeNotifications.Api.EventUsersProviders;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EchoSphere.RealtimeNotifications.Api.Extensions;

internal static class ServiceCollectionExtensions
{
	public static IServiceCollection AddEventToUsersSender<TEvent, TProvider>(this IServiceCollection services)
		where TEvent : IIntegrationEvent
		where TProvider : class, IEventUsersProvider<TEvent>
	{
		services.TryAddScoped<TProvider>();
		services.TryAddScoped<IIntegrationEventHandler<TEvent>, EventToUsersSender<TEvent, TProvider>>();

		return services;
	}
}