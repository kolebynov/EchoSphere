using EchoSphere.BlazorShared.Extensions;
using MudBlazor;
using MudBlazor.Services;

namespace EchoSphere.Accounts.WebApp.Client.Extensions;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddCommonClientAndServerServices(this IServiceCollection services)
	{
		services.AddSharedBlazor();

		return services;
	}
}