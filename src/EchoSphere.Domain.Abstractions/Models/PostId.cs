namespace EchoSphere.Domain.Abstractions.Models;

public readonly record struct PostId(Guid Value) : IIdValue<Guid>;