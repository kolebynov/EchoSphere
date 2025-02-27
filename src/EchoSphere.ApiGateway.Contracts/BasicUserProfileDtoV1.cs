namespace EchoSphere.ApiGateway.Contracts;

public sealed class BasicUserProfileDtoV1
{
	public required Guid Id { get; init; }

	public required string FirstName { get; init; }

	public required string SecondName { get; init; }
}