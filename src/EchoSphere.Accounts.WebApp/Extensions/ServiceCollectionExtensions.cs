using EchoSphere.Accounts.WebApp.Models;
using EchoSphere.Accounts.WebApp.Services;

namespace EchoSphere.Accounts.WebApp.Extensions;

internal static class ServiceCollectionExtensions
{
	public static IdentityBuilder AddLinqToDbStores(this IdentityBuilder identityBuilder)
	{
		return identityBuilder
			.AddUserStore<LingToDbUserStore<
				Guid, Account, Role, IdentityUserClaim<Guid>, IdentityUserRole<Guid>,
				IdentityUserLogin<Guid>, IdentityUserToken<Guid>, IdentityRoleClaim<Guid>
			>>()
			.AddRoleStore<LingToDbRoleStore<Role, Guid, IdentityUserRole<Guid>, IdentityRoleClaim<Guid>>>();
	}
}