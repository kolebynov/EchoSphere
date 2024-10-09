namespace EchoSphere.Domain.Abstractions.Models;

public readonly record struct UserId(Guid Value) : IIdValue<Guid>;