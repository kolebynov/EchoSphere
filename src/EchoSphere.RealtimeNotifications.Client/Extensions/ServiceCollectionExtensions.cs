using EchoSphere.Domain.Abstractions.Extensions;
using EchoSphere.RealtimeNotifications.Client.Settings;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace EchoSphere.RealtimeNotifications.Client.Extensions;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddRealtimeNotificationsClient(this IServiceCollection services)
	{
		services.TryAddSingleton(sp =>
		{
			var settings = sp.GetRequiredService<IOptions<RealtimeNotificationsClientSettings>>().Value;
			return new HubConnectionBuilder()
				.WithUrl(settings.Url)
				.AddJsonProtocol(opt => opt.PayloadSerializerOptions.AddDomainConverters())
				.WithAutomaticReconnect()
				.Build();
		});
		services.TryAddSingleton<IRealtimeNotificationsClient, RealtimeNotificationsClient>();

		return services;
	}
}