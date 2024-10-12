using EchoSphere.GrpcClientShared.Extensions;
using EchoSphere.Notifications.Abstractions;
using EchoSphere.Notifications.Grpc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EchoSphere.Notifications.Client.Extensions;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddNotificationsGrpcClient(this IServiceCollection services, Uri address)
	{
		services.AddGrpcClientWithExecutor<NotificationService.NotificationServiceClient>(o => o.Address = address);

		services.TryAddScoped<INotificationService, NotificationGrpcClient>();

		return services;
	}
}