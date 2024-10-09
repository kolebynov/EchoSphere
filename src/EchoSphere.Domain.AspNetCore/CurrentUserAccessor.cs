using System.Security.Claims;
using EchoSphere.Domain.Abstractions;
using EchoSphere.Domain.Abstractions.Extensions;
using EchoSphere.Domain.Abstractions.Models;
using Microsoft.AspNetCore.Http;

namespace EchoSphere.Domain.AspNetCore;

public sealed class CurrentUserAccessor : ICurrentUserAccessor
{
	private readonly IHttpContextAccessor _httpContextAccessor;
	private UserId _currentUserId;

	public UserId CurrentUserId
	{
		get
		{
			if (_currentUserId != default)
			{
				return _currentUserId;
			}

			var httpContext = _httpContextAccessor.HttpContext;
			if (httpContext == null)
			{
				throw new InvalidOperationException("No http context was found.");
			}

			var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
			if (userIdClaim == null)
			{
				throw new InvalidOperationException("No user id claim was found.");
			}

			return _currentUserId = IdValueExtensions.Parse<UserId>(userIdClaim.Value);
		}
	}

	public CurrentUserAccessor(IHttpContextAccessor httpContextAccessor)
	{
		_httpContextAccessor = httpContextAccessor;
	}
}