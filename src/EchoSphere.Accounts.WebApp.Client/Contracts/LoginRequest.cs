namespace EchoSphere.Accounts.WebApp.Client.Contracts;

public sealed class LoginRequest
{
	public required string UserName { get; init; }

	public required string Password { get; init; }

#pragma warning disable CA1056
	public required string ReturnUrl { get; init; }
#pragma warning restore CA1056
}