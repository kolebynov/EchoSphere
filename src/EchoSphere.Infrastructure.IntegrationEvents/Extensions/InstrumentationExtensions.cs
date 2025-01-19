using OpenTelemetry.Trace;

namespace EchoSphere.Infrastructure.IntegrationEvents.Extensions;

public static class InstrumentationExtensions
{
	public static TracerProviderBuilder AddIntegrationEventInstrumentation(this TracerProviderBuilder builder)
	{
		builder.AddSource(ActivitySources.Source.Name);
		return builder;
	}
}