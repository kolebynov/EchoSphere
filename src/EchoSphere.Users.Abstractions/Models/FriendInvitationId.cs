using EchoSphere.Domain.Abstractions.Models;

namespace EchoSphere.Users.Abstractions.Models;

public readonly record struct FriendInvitationId(Guid Value) : IIdValue<Guid>;