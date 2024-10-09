using EchoSphere.Domain.Abstractions.Models;

namespace EchoSphere.Domain.Abstractions;

public interface ICurrentUserAccessor
{
	UserId CurrentUserId { get; }
}