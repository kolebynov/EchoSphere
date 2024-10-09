namespace EchoSphere.Domain.Abstractions.Models;

public interface IIdValue<TValue>
{
	TValue Value { get; init; }
}