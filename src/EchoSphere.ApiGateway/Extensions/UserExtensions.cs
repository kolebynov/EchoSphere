using System.Security.Claims;
using EchoSphere.Domain.Abstractions.Extensions;
using EchoSphere.Domain.Abstractions.Models;

namespace EchoSphere.ApiGateway.Extensions;

public static class UserExtensions
{
	// TODO: Return Option<UserId>
	public static UserId GetUserId(this ClaimsPrincipal principal) =>
		IdValueExtensions.Parse<UserId>(principal.FindFirstValue(ClaimTypes.NameIdentifier)!);
}