namespace EchoSphere.Accounts.WebApp.Client.Contracts;

public sealed class RegisterRequest
{
	public required string UserName { get; init; }

	public required string Email { get; init; }

	public required string Password { get; init; }
}