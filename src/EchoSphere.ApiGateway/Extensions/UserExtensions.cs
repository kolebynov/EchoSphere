using System.Security.Claims;
using EchoSphere.Users.Abstractions.Models;

namespace EchoSphere.ApiGateway.Extensions;

public static class UserExtensions
{
	public static UserId GetUserId(this ClaimsPrincipal principal) =>
		new UserId(Guid.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier)!));
}