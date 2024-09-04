using EchoSphere.UsersApi.Models;
using EchoSphere.UsersApi.Services;
using Microsoft.AspNetCore.Identity;

namespace EchoSphere.UsersApi.Extensions;

internal static class ServiceCollectionExtensions
{
	public static IdentityBuilder AddLinqToDbStores(this IdentityBuilder identityBuilder)
	{
		return identityBuilder
			.AddUserStore<LingToDbUserStore<
				Guid, UserProfile, UserRole, IdentityUserClaim<Guid>, IdentityUserRole<Guid>,
				IdentityUserLogin<Guid>, IdentityUserToken<Guid>, IdentityRoleClaim<Guid>
			>>()
			.AddRoleStore<LingToDbRoleStore<UserRole, Guid, IdentityUserRole<Guid>, IdentityRoleClaim<Guid>>>();
	}
}