using EchoSphere.SharedModels;

namespace EchoSphere.Messages.Abstractions.Models;

public readonly record struct MessageId(long Value) : IIdValue<long>;