using EchoSphere.Domain.Abstractions.Models;

namespace EchoSphere.Posts.Abstractions.Models;

public readonly record struct PostCommentId(Guid Value) : IIdValue<Guid>;