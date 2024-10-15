using System.Security.Claims;
using EchoSphere.ApiGateway.Client;
using EchoSphere.BlazorShared;
using Microsoft.AspNetCore.Components.Authorization;

namespace EchoSphere.Web.Client;

internal sealed class UserProfileAuthenticationStateProvider : AuthenticationStateProvider
{
	private const string UserIdClaimType = "sub";
	private const string NameClaimType = "name";

	private readonly IUserProfileClient _userProfileClient;
	private readonly LongOperationExecutor _longOperationExecutor;

	public UserProfileAuthenticationStateProvider(
		IUserProfileClient userProfileClient, LongOperationExecutor longOperationExecutor)
	{
		_userProfileClient = userProfileClient;
		_longOperationExecutor = longOperationExecutor;
	}

	public override async Task<AuthenticationState> GetAuthenticationStateAsync()
	{
		var claimsPrincipal = await _longOperationExecutor.ExecuteLongOperationAndReturnDefaultIfError(
			async () =>
			{
				var currentUserProfile = await _userProfileClient.GetUserProfile("me", CancellationToken.None);
				Claim[] claims =
				[
					new(UserIdClaimType, currentUserProfile.Id.ToString()),
					new(NameClaimType, $"{currentUserProfile.FirstName} {currentUserProfile.SecondName}"),
				];

				return new ClaimsPrincipal(new ClaimsIdentity(
					claims, nameof(UserProfileAuthenticationStateProvider), NameClaimType, null));
			}, new ClaimsPrincipal(new ClaimsIdentity()));

		return new AuthenticationState(claimsPrincipal);
	}
}