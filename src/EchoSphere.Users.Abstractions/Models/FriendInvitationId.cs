using EchoSphere.SharedModels;

namespace EchoSphere.Users.Abstractions.Models;

public readonly record struct FriendInvitationId(Guid Value) : IIdValue<Guid>;