using EchoSphere.Domain.Abstractions.Models;

namespace EchoSphere.Domain.Abstractions;

public sealed class SpecificUserAccessor : ICurrentUserAccessor
{
	public UserId CurrentUserId { get; }

	public SpecificUserAccessor(UserId currentUserId)
	{
		CurrentUserId = currentUserId;
	}
}