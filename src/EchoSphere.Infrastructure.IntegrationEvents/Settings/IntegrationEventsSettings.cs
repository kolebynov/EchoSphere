namespace EchoSphere.Infrastructure.IntegrationEvents.Settings;

public sealed class IntegrationEventsSettings
{
	private string? _serviceName;

	public bool DisableProducer { get; set; }

	public bool DisableConsumer { get; set; }

	public int BatchSize { get; set; } = 50;

	public string? ServiceName
	{
		get => _serviceName;
		set
		{
			_serviceName = value;
			TopicName = GetTopicName(_serviceName);
		}
	}

	public IList<string> ListenServiceNames { get; } = [];

	internal string? TopicName { get; private set; }

	internal IEnumerable<string> ListenTopicNames => ListenServiceNames.Select(GetTopicName)!;

	private static string? GetTopicName(string? serviceName) =>
		!string.IsNullOrEmpty(serviceName) ? $"integration_events-{serviceName}" : null;
}