using EchoSphere.Domain.Abstractions.Models;

namespace EchoSphere.Notifications.Abstractions.Models;

public readonly record struct NotificationId(long Value) : IIdValue<long>;