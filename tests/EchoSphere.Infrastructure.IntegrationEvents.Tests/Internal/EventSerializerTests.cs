using EchoSphere.Domain.Abstractions.Models;
using EchoSphere.Infrastructure.IntegrationEvents.Internal;
using FluentAssertions;

namespace EchoSphere.Infrastructure.IntegrationEvents.Tests.Internal;

public class EventSerializerTests
{
	[Fact]
	public void Serialize_ForEventsWithIdValues_SerializeCorrectly()
	{
		// Arrange

		var expectedResult = new SerializedIntegrationEvent
		{
			TypeName = "EchoSphere.Infrastructure.IntegrationEvents.Tests.Internal.EventSerializerTests+TestEvent, EchoSphere.Infrastructure.IntegrationEvents.Tests",
			EventData = [123, 34, 83, 116, 114, 86, 97, 108, 117, 101, 34, 58, 34, 116, 101, 115, 116, 34, 44, 34, 73, 100, 86, 97, 108, 117, 101, 34, 58, 49, 48, 125],
		};

		var testEvent = new TestEvent
		{
			IdValue = new TestIdValue { Value = 10 },
			StrValue = "test",
		};

		var target = new EventSerializer();

		// Act

		var result = target.Serialize(testEvent);

		// Assert

		result.TypeName.Should().Be(expectedResult.TypeName);
		result.EventData.Should().BeEquivalentTo(expectedResult.EventData);
	}

	[Fact]
	public void Deserialize_ForSerializedEventWithIdValues_DeserializeCorrectly()
	{
		// Arrange

		var serializedEvent = new SerializedIntegrationEvent
		{
			TypeName = "EchoSphere.Infrastructure.IntegrationEvents.Tests.Internal.EventSerializerTests+TestEvent, EchoSphere.Infrastructure.IntegrationEvents.Tests",
			EventData = [123, 34, 83, 116, 114, 86, 97, 108, 117, 101, 34, 58, 34, 116, 101, 115, 116, 34, 44, 34, 73, 100, 86, 97, 108, 117, 101, 34, 58, 49, 48, 125],
		};

		var expectedTestEvent = new TestEvent
		{
			IdValue = new TestIdValue { Value = 10 },
			StrValue = "test",
		};

		var target = new EventSerializer();

		// Act

		var result = target.Deserialize(serializedEvent);

		// Assert

		result.Should().BeEquivalentTo(expectedTestEvent);
	}

	public sealed class TestEvent : IIntegrationEvent
	{
		public required string StrValue { get; init; }

		public required TestIdValue IdValue { get; init; }
	}

	public sealed class TestIdValue : IIdValue<long>
	{
		public long Value { get; init; }
	}
}