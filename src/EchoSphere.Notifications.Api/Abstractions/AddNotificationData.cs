using EchoSphere.Domain.Abstractions.Models;

namespace EchoSphere.Notifications.Api.Abstractions;

internal record struct AddNotificationData(UserId UserId, string NotificationBody);