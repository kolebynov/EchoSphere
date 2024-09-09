using System.Reflection;
using EchoSphere.Accounts.WebApp.Client.Contracts;
using EchoSphere.Accounts.WebApp.Client.Extensions;
using EchoSphere.Accounts.WebApp.Components;
using EchoSphere.Accounts.WebApp.Configuration;
using EchoSphere.Accounts.WebApp.Data;
using EchoSphere.Accounts.WebApp.Extensions;
using EchoSphere.Accounts.WebApp.Models;
using EchoSphere.Accounts.WebApp.Services;
using EchoSphere.Infrastructure.Db.Extensions;
using EchoSphere.Infrastructure.Hosting.Extensions;
using LinqToDB.Mapping;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddRazorComponents()
	.AddInteractiveServerComponents()
	.AddInteractiveWebAssemblyComponents();

var mappingSchema = new MappingSchema();
var fluentMappingBuilder = new FluentMappingBuilder(mappingSchema);

fluentMappingBuilder.Entity<Account>()
	.HasTableName(DataConstants.AccountsTableName)
	.HasPrimaryKey(x => x.Id);

fluentMappingBuilder.Entity<IdentityUserClaim<Guid>>()
	.HasTableName(DataConstants.AccountClaimsTableName)
	.HasPrimaryKey(x => x.Id);

fluentMappingBuilder.Entity<Role>()
	.HasTableName(DataConstants.RolesTableName)
	.HasPrimaryKey(x => x.Id);

fluentMappingBuilder.Entity<IdentityUserRole<Guid>>()
	.HasTableName(DataConstants.AccountRolesTableName);

fluentMappingBuilder.Build();

builder.Services.AddLinqToDb<AppDataConnection>(builder.Configuration, "AccountsDb", mappingSchema,
	Assembly.GetExecutingAssembly());

builder.Services.AddIdentity<Account, Role>(
		opt =>
		{
			opt.Password.RequireDigit = false;
			opt.Password.RequireLowercase = false;
			opt.Password.RequireNonAlphanumeric = false;
			opt.Password.RequireUppercase = false;
			opt.Password.RequiredLength = 4;
		})
	.AddLinqToDbStores()
	.AddDefaultTokenProviders();

builder.Services
	.AddIdentityServer(options =>
	{
		options.Authentication.CookieLifetime = TimeSpan.FromHours(2);

		options.Events.RaiseErrorEvents = true;
		options.Events.RaiseInformationEvents = true;
		options.Events.RaiseFailureEvents = true;
		options.Events.RaiseSuccessEvents = true;

		// TODO: Remove this line in production.
		options.KeyManagement.Enabled = false;

		options.UserInteraction.LoginUrl = "/login";
	})
	.AddInMemoryIdentityResources(Config.GetResources())
	.AddInMemoryApiScopes(Config.GetApiScopes())
	.AddInMemoryApiResources(Config.GetApis())
	.AddInMemoryClients(Config.GetClients(builder.Configuration))
	.AddAspNetIdentity<Account>()
	// TODO: Not recommended for production - you need to store your key material somewhere secure
	.AddDeveloperSigningCredential();

builder.Services.AddCommonClientAndServerServices();

builder.Services.AddTransient<IProfileService, ProfileService>();

// TODO: Remove later
builder.Services.AddScopedAsyncInitializer(async (sp, _) =>
{
	var userManager = sp.GetRequiredService<UserManager<Account>>();
	await userManager.CreateAsync(
		new Account
		{
			Id = new Guid("cd2ecf42-deb8-4381-9567-3754cea3465a"),
			UserName = "test1",
			Email = "test1@test.com",
		},
		"1234");
	await userManager.CreateAsync(
		new Account
		{
			Id = new Guid("e6da2482-630f-4755-9b13-f4b4733c0ad5"),
			UserName = "test2",
			Email = "test2@test.com",
		},
		"1234");
});

var app = builder.Build();

app.UseStaticFiles();

app.UseCookiePolicy(new CookiePolicyOptions { MinimumSameSitePolicy = SameSiteMode.Lax });
app.UseRouting();
app.UseIdentityServer();
app.UseAuthorization();
app.UseAntiforgery();

app.MapPost(
	"/api/login",
	async ([FromBody] LoginRequest request, IIdentityServerInteractionService interaction, SignInManager<Account> signInManager, UserManager<Account> userManager) =>
	{
		var context = await interaction.GetAuthorizationContextAsync(request.ReturnUrl);

		var result = await signInManager.PasswordSignInAsync(request.UserName, request.Password, false, lockoutOnFailure: true);
		if (!result.Succeeded)
		{
			return Results.BadRequest("Invalid user name or password");
		}

		if (context != null || Uri.TryCreate(request.ReturnUrl, UriKind.Relative, out _) || string.IsNullOrEmpty(request.ReturnUrl))
		{
			return Results.Ok();
		}

		return Results.BadRequest("Invalid redirect url");
	});

app.MapPost(
	"api/register",
	async (RegisterRequest request, UserManager<Account> userManager) =>
	{
		var result = await userManager.CreateAsync(
			new Account
			{
				UserName = request.UserName,
				Email = request.Email,
			},
			request.Password);
		return !result.Succeeded ? Results.BadRequest(result.Errors.First().Description) : Results.Ok();
	});

app.MapRazorComponents<App>()
	.AddInteractiveWebAssemblyRenderMode()
	.AddInteractiveServerRenderMode()
	.AddAdditionalAssemblies(typeof(EchoSphere.Accounts.WebApp.Client.Components._Imports).Assembly);

await app.InitAndRunAsync();
