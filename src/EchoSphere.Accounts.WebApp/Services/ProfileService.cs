using Duende.IdentityServer.AspNetIdentity;
using EchoSphere.Accounts.WebApp.Models;

namespace EchoSphere.Accounts.WebApp.Services;

internal sealed class ProfileService : ProfileService<Account>
{
	public ProfileService(UserManager<Account> userManager, IUserClaimsPrincipalFactory<Account> claimsFactory)
		: base(userManager, claimsFactory)
	{
	}

	public ProfileService(UserManager<Account> userManager, IUserClaimsPrincipalFactory<Account> claimsFactory,
		ILogger<ProfileService<Account>> logger)
		: base(userManager, claimsFactory, logger)
	{
	}
}