using EchoSphere.Domain.Abstractions.Models;

namespace EchoSphere.Users.Abstractions.Models;

public sealed class BasicUserProfile
{
	public required UserId Id { get; init; }

	public required string FirstName { get; init; }

	public required string SecondName { get; init; }
}