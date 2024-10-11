using Confluent.Kafka;

namespace EchoSphere.Infrastructure.IntegrationEvents.Internal;

internal readonly struct MessagesPoller<TKey, TValue>
{
	private readonly IConsumer<TKey, TValue> _consumer;
	private readonly ConsumeResult<TKey, TValue>[] _batch;

	public MessagesPoller(IConsumer<TKey, TValue> consumer, int batchSize)
	{
		_consumer = consumer;
		_batch = new ConsumeResult<TKey, TValue>[batchSize];
	}

	public ArraySegment<ConsumeResult<TKey, TValue>> PollBatch(CancellationToken cancellationToken)
	{
		_batch[0] = _consumer.Consume(cancellationToken)!;

		var index = 1;
		for (; index < _batch.Length && _consumer.Consume(1) is { } consumeResultNext; index++)
		{
			_batch[index] = consumeResultNext;
		}

		return new ArraySegment<ConsumeResult<TKey, TValue>>(_batch, 0, index);
	}
}