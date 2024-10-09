namespace EchoSphere.Domain.Abstractions.Models;

public readonly record struct ChatId(Guid Value) : IIdValue<Guid>;