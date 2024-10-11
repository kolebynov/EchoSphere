using System.Diagnostics;
using System.Text;
using Confluent.Kafka;

namespace EchoSphere.Infrastructure.IntegrationEvents.Internal;

internal sealed class IntegrationEventKafkaSerializer : ISerializer<SerializedIntegrationEvent>, IDeserializer<SerializedIntegrationEvent>
{
	private const byte Semicolon = 59;

	public byte[] Serialize(SerializedIntegrationEvent data, SerializationContext context)
	{
		var typeNameLength = Encoding.UTF8.GetByteCount(data.TypeName);
		var serializedMessageData = new byte[typeNameLength + 1 + data.EventData.Length];
		Debug.Assert(Encoding.UTF8.GetBytes(data.TypeName, serializedMessageData) == typeNameLength, "Encoding.UTF8.GetBytes(data.TypeName, serializedMessageData) == typeNameLength");
		serializedMessageData[typeNameLength] = Semicolon;
		data.EventData.CopyTo(serializedMessageData.AsSpan(typeNameLength + 1));

		return serializedMessageData;
	}

	public SerializedIntegrationEvent Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
	{
		if (isNull)
		{
			return default;
		}

		var semicolonIndex = data.IndexOf(Semicolon);
		if (semicolonIndex < 0)
		{
			throw new ArgumentException("Invalid incoming message", nameof(data));
		}

		var typeName = Encoding.UTF8.GetString(data[..semicolonIndex]);
		return new SerializedIntegrationEvent
		{
			TypeName = typeName,
			EventData = data[(semicolonIndex + 1)..].ToArray(),
		};
	}
}