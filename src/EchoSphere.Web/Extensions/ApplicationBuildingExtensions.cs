using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.IdentityModel.JsonWebTokens;

namespace EchoSphere.Web.Extensions;

public static class ApplicationBuildingExtensions
{
	public static void AddApplicationServices(this IHostApplicationBuilder builder)
	{
		builder.AddAuthenticationServices();
	}

	public static void AddAuthenticationServices(this IHostApplicationBuilder builder)
	{
		var configuration = builder.Configuration;
		var services = builder.Services;

		JsonWebTokenHandler.DefaultInboundClaimTypeMap.Remove("sub");

		var identityUrl = configuration.GetValue<string>("IdentityUrl");
		var callBackUrl = configuration.GetValue<string>("CallBackUrl");
		var sessionCookieLifetime = configuration.GetValue("SessionCookieLifetimeMinutes", 60);

		services.AddAuthorization();
		services
			.AddAuthentication(options =>
			{
				options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
				options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
			})
			.AddCookie(options =>
			{
				options.ExpireTimeSpan = TimeSpan.FromMinutes(sessionCookieLifetime);
			})
			.AddOpenIdConnect(options =>
			{
				options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
				options.Authority = identityUrl;
				options.SignedOutRedirectUri = callBackUrl!;
				options.ClientId = "webapp";
				options.ClientSecret = "secret";
				options.ResponseType = "code";
				options.SaveTokens = true;
				options.GetClaimsFromUserInfoEndpoint = true;
				options.RequireHttpsMetadata = false;
				options.Scope.Add("openid");
				options.Scope.Add("profile");
				options.Scope.Add("api_gateway");
			});

		services.AddScoped<AuthenticationStateProvider, ServerAuthenticationStateProvider>();
		services.AddCascadingAuthenticationState();
	}
}