namespace EchoSphere.SharedModels;

public interface IIdValue<TValue>
{
	TValue Value { get; init; }
}