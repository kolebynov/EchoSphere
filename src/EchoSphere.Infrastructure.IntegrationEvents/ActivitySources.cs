using System.Diagnostics;

namespace EchoSphere.Infrastructure.IntegrationEvents;

internal static class ActivitySources
{
	public static readonly ActivitySource Source = new("EchoSphere.Infrastructure.IntegrationEvents", "1.0.0");
}