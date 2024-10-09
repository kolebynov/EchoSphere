using System.Security.Claims;
using System.Text.Encodings.Web;
using EchoSphere.Domain.Abstractions.Extensions;
using EchoSphere.Domain.Abstractions.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EchoSphere.Domain.AspNetCore.Authentication;

internal sealed class UserIdAuthenticationHandler : AuthenticationHandler<UserIdAuthenticationOptions>
{
	public UserIdAuthenticationHandler(IOptionsMonitor<UserIdAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder)
		: base(options, logger, encoder)
	{
	}

	protected override Task<AuthenticateResult> HandleAuthenticateAsync()
	{
		var userIdStr = Context.Request.Headers.Authorization.ToString();
		var result = IdValueExtensions.TryParse<UserId>(userIdStr)
			.Map(_ =>
			{
				Claim[] claims =
				[
					new(ClaimTypes.NameIdentifier, userIdStr),
					new(ClaimTypes.Name, userIdStr),
				];
				var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims, Scheme.Name));
				return AuthenticateResult.Success(new AuthenticationTicket(claimsPrincipal, Scheme.Name));
			})
			.IfNone(() => AuthenticateResult.Fail("No user id provided."));

		return Task.FromResult(result);
	}
}