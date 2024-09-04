using Extensions.Hosting.AsyncInitialization;
using Microsoft.Extensions.DependencyInjection;

namespace EchoSphere.Infrastructure.Hosting.Extensions;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddScopedAsyncInitializer(
		this IServiceCollection services, Func<IServiceProvider, CancellationToken, Task> initializer) =>
		services.AddAsyncInitializer(sp => new ScopedAsyncInitializer(sp, initializer));

	private sealed class ScopedAsyncInitializer : IAsyncInitializer
	{
		private readonly IServiceProvider _serviceProvider;
		private readonly Func<IServiceProvider, CancellationToken, Task> _initializer;

		public ScopedAsyncInitializer(
			IServiceProvider serviceProvider, Func<IServiceProvider, CancellationToken, Task> initializer)
		{
			_serviceProvider = serviceProvider;
			_initializer = initializer;
		}

		public async Task InitializeAsync(CancellationToken cancellationToken)
		{
			using var scope = _serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope();
			await _initializer(scope.ServiceProvider, cancellationToken);
		}
	}
}