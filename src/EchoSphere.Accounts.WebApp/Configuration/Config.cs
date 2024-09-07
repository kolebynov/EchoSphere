namespace EchoSphere.Accounts.WebApp.Configuration;

public static class Config
{
	public static IEnumerable<ApiResource> GetApis()
	{
		return new List<ApiResource>
		{
			new("api_gateway", "API gateway"),
		};
	}

	public static IEnumerable<ApiScope> GetApiScopes()
	{
		return new List<ApiScope>
		{
			new("api_gateway", "API gateway"),
		};
	}

	// Identity resources are data like user ID, name, or email address of a user
	// see: http://docs.identityserver.io/en/release/configuration/resources.html
	public static IEnumerable<IdentityResource> GetResources()
	{
		return new List<IdentityResource>
		{
			new IdentityResources.OpenId(),
			new IdentityResources.Profile(),
		};
	}

	// client want to access resources (aka scopes)
	public static IEnumerable<Duende.IdentityServer.Models.Client> GetClients(IConfiguration configuration)
	{
		return new List<Duende.IdentityServer.Models.Client>
		{
			new()
			{
				ClientId = "webapp",
				ClientName = "WebApp Client",
				ClientSecrets = new List<Secret>
				{
					new("secret".Sha256()),
				},
				ClientUri = $"{configuration["WebAppClient"]}", // public uri of the client
				AllowedGrantTypes = GrantTypes.Code,
				AllowAccessTokensViaBrowser = false,
				RequireConsent = false,
				AllowOfflineAccess = true,
				AlwaysIncludeUserClaimsInIdToken = true,
				RequirePkce = false,
				RedirectUris = new List<string>
				{
					$"{configuration["WebAppClient"]}/signin-oidc",
				},
				PostLogoutRedirectUris = new List<string>
				{
					$"{configuration["WebAppClient"]}/signout-callback-oidc",
				},
				AllowedScopes = new List<string>
				{
					IdentityServerConstants.StandardScopes.OpenId,
					IdentityServerConstants.StandardScopes.Profile,
					IdentityServerConstants.StandardScopes.OfflineAccess,
					"api_gateway",
				},
				AccessTokenLifetime = 60 * 60 * 2, // 2 hours
				IdentityTokenLifetime = 60 * 60 * 2, // 2 hours
			},
		};
	}
}