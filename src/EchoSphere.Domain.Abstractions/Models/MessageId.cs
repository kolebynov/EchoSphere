namespace EchoSphere.Domain.Abstractions.Models;

public readonly record struct MessageId(long Value) : IIdValue<long>;