using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EchoSphere.Infrastructure.Hosting;

#pragma warning disable CA1001
public abstract class BaseHostedService : IHostedService
#pragma warning restore CA1001
{
	private readonly CancellationTokenSource _cancellationTokenSource = new();
	private readonly IServiceScopeFactory _serviceScopeFactory;
	private IServiceScope? _scope;
	private Task? _runTask;

	protected BaseHostedService(IServiceScopeFactory serviceScopeFactory)
	{
		_serviceScopeFactory = serviceScopeFactory;
	}

	public Task StartAsync(CancellationToken cancellationToken)
	{
		_scope = _serviceScopeFactory.CreateScope();
		var logger = (ILogger)_scope.ServiceProvider.GetRequiredService(typeof(ILogger<>).MakeGenericType(GetType()));
		_runTask = Task.Run(
			() => RunAsync(_scope.ServiceProvider, _cancellationTokenSource.Token)
				.ContinueWith(
					t =>
					{
						if (t.IsFaulted)
						{
							logger.LogWarning(t.Exception, "Hosted service fails");
						}
					},
					_cancellationTokenSource.Token, TaskContinuationOptions.None, TaskScheduler.Current),
			_cancellationTokenSource.Token);
		return Task.CompletedTask;
	}

	public async Task StopAsync(CancellationToken cancellationToken)
	{
		using var cts = _cancellationTokenSource;
		using var s = _scope;

		await _cancellationTokenSource.CancelAsync();

		try
		{
			await _runTask!;
		}
		catch (OperationCanceledException)
		{
		}
	}

	protected abstract Task RunAsync(
		IServiceProvider scopeServiceProvider, CancellationToken stopCancellationToken);
}