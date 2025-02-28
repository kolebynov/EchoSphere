using EchoSphere.Infrastructure.IntegrationEvents;
using EchoSphere.Infrastructure.IntegrationEvents.Abstractions;
using EchoSphere.Notifications.Api.Abstractions;
using EchoSphere.Notifications.Api.Services;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EchoSphere.Notifications.Api.Extensions;

internal static class ServiceCollectionExtensions
{
	public static IServiceCollection AddEventToNotificationHandler<TEvent, TConverter>(
		this IServiceCollection services)
		where TEvent : IIntegrationEvent
		where TConverter : class, IEventToNotificationsConverter<TEvent>
	{
		services.TryAddScoped<TConverter>();
		services.TryAddScoped<IIntegrationEventHandler<TEvent>, EventToNotificationsHandler<TEvent, TConverter>>();

		return services;
	}
}