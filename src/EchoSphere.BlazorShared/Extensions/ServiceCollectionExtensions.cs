using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;

namespace EchoSphere.BlazorShared.Extensions;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddSharedBlazor(this IServiceCollection services) =>
		services
			.AddLocalization()
			.AddMudServices(opt => { opt.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomCenter; })
			.AddScoped<ErrorPresenter>()
			.AddScoped<LoaderOverlayService>()
			.AddScoped<LongOperationExecutor>();
}