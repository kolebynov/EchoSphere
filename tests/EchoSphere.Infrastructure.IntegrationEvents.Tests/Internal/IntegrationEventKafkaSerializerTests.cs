using EchoSphere.Infrastructure.IntegrationEvents.Internal;
using FluentAssertions;

namespace EchoSphere.Infrastructure.IntegrationEvents.Tests.Internal;

public class IntegrationEventKafkaSerializerTests
{
	[Fact]
	public void Serialize_ForIntegrationEvent_SerializeCorrectly()
	{
		// Arrange
		var @event = new SerializedIntegrationEvent
		{
			TypeName = "TypeName",
			EventData = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10],
		};

		byte[] expectedResult = [84, 121, 112, 101, 78, 97, 109, 101, 59, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10];

		var target = new IntegrationEventKafkaSerializer();

		// Act

		var result = target.Serialize(@event, default);

		// Assert

		result.Should().BeEquivalentTo(expectedResult);
	}

	[Fact]
	public void Deserialize_ForIntegrationEventByteData_DeserializeCorrectly()
	{
		// Arrange

		byte[] eventData = [84, 121, 112, 101, 78, 97, 109, 101, 59, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10];

		var expectedEvent = new SerializedIntegrationEvent
		{
			TypeName = "TypeName",
			EventData = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10],
		};

		var target = new IntegrationEventKafkaSerializer();

		// Act

		var result = target.Deserialize(eventData, false, default);

		// Assert

		result.TypeName.Should().Be(expectedEvent.TypeName);
		result.EventData.Should().BeEquivalentTo(expectedEvent.EventData);
	}
}