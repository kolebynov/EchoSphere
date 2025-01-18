using EchoSphere.Domain.Abstractions;
using EchoSphere.Domain.Abstractions.Extensions;
using Grpc.Net.ClientFactory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EchoSphere.GrpcClientShared.Extensions;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddGrpcClientWithExecutor<TClient>(
		this IServiceCollection services, Action<GrpcClientFactoryOptions> configureClient)
		where TClient : class
	{
		services
			.AddGrpcClient<TClient>(configureClient)
			.AddCallCredentials((_, metadata, serviceProvider) =>
			{
				var currentUserId = serviceProvider.GetRequiredService<ICurrentUserAccessor>().CurrentUserId;
				metadata.Add("Authorization", currentUserId.ToInnerString());
				return Task.CompletedTask;
			});
		services.TryAddTransient<GrpcCallExecutor<TClient>>();

		return services;
	}
}