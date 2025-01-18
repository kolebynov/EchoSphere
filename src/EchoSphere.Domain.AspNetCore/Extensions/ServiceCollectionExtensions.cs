using EchoSphere.Domain.Abstractions;
using EchoSphere.Domain.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EchoSphere.Domain.AspNetCore.Extensions;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddDomainServices(this IServiceCollection services)
	{
		services.AddAuthentication(UserIdAuthenticationDefaults.AuthenticationScheme)
			.AddScheme<UserIdAuthenticationOptions, UserIdAuthenticationHandler>(UserIdAuthenticationDefaults.AuthenticationScheme, null);
		services.AddAuthorization();

		return services.AddDomainServicesCore();
	}

	public static IServiceCollection AddDomainServicesCore(this IServiceCollection services)
	{
		services.AddHttpContextAccessor();

		services.TryAddScoped<ICurrentUserAccessor, HttpContextUserAccessor>();

		return services;
	}
}