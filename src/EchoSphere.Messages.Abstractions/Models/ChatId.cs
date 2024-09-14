using EchoSphere.SharedModels;

namespace EchoSphere.Messages.Abstractions.Models;

public readonly record struct ChatId(Guid Value) : IIdValue<Guid>;