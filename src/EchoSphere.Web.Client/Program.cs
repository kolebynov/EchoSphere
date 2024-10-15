using EchoSphere.ApiGateway.Client;
using EchoSphere.BlazorShared.Extensions;
using EchoSphere.Web.Client;
using EchoSphere.Web.Client.Extensions;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Http.Resilience;
using Refit;

if (!OperatingSystem.IsBrowser())
{
	return;
}

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.ConfigureHttpClientDefaults(http =>
{
	http.AddStandardResilienceHandler().Configure(opt =>
	{
		opt.AttemptTimeout.Timeout = TimeSpan.FromSeconds(30);
		opt.CircuitBreaker.SamplingDuration = opt.AttemptTimeout.Timeout * 2;
	});
});

builder.Services.AddCommonClientAndServerServices();
builder.Services.AddCascadingAuthenticationState();

var apiUri = new Uri(new Uri(builder.HostEnvironment.BaseAddress), "api");
Console.WriteLine(apiUri);
builder.Services.AddRefitClient<IChatClient>()
	.ConfigureHttpClient(client => client.BaseAddress = apiUri)
	.IncludeBrowserCredentials();
builder.Services.AddRefitClient<IUserProfileClient>()
	.ConfigureHttpClient(client => client.BaseAddress = apiUri)
	.IncludeBrowserCredentials();
builder.Services.AddRefitClient<IFriendClient>()
	.ConfigureHttpClient(client => client.BaseAddress = apiUri)
	.IncludeBrowserCredentials();
builder.Services.AddRefitClient<IFollowClient>()
	.ConfigureHttpClient(client => client.BaseAddress = apiUri)
	.IncludeBrowserCredentials();
builder.Services.AddRefitClient<IPostClient>()
	.ConfigureHttpClient(client => client.BaseAddress = apiUri)
	.IncludeBrowserCredentials();
builder.Services.AddRefitClient<INotificationClient>()
	.ConfigureHttpClient(client => client.BaseAddress = apiUri)
	.IncludeBrowserCredentials();

builder.Services.AddScoped<AuthenticationStateProvider, UserProfileAuthenticationStateProvider>();

var host = builder.Build();

await host.RunAsync();