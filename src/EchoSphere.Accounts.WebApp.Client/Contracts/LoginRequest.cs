namespace EchoSphere.Accounts.WebApp.Client.Contracts;

public sealed class LoginRequest
{
	public required string UserName { get; init; }

	public required string Password { get; init; }

	public required string ReturnUrl { get; init; }
}