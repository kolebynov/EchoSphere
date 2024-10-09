using System.Security.Claims;
using Grpc.Net.ClientFactory;
using Microsoft.AspNetCore.Http;
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
				var userIdClaim = serviceProvider.GetRequiredService<IHttpContextAccessor>().HttpContext?.User
					.FindFirst(ClaimTypes.NameIdentifier);
				if (userIdClaim == null)
				{
					return Task.CompletedTask;
				}

				metadata.Add("Authorization", userIdClaim.Value);
				return Task.CompletedTask;
			});
		services.TryAddTransient<GrpcCallExecutor<TClient>>();

		return services;
	}
}