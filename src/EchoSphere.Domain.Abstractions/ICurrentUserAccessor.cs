using EchoSphere.Domain.Abstractions.Models;

namespace EchoSphere.Domain.Abstractions;

public interface ICurrentUserAccessor
{
	public static readonly UserId SupervisorUserId = new(new Guid("410006e1-ca4e-4502-a000-e00000000000"));

	public bool IsSupervisor => CurrentUserId == SupervisorUserId;

	UserId CurrentUserId { get; }
}