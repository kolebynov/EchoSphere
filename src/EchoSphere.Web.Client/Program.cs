using EchoSphere.ApiGateway.Client;
using EchoSphere.BlazorShared.Extensions;
using EchoSphere.RealtimeNotifications.Client.Extensions;
using EchoSphere.RealtimeNotifications.Client.Settings;
using EchoSphere.Web.Client;
using EchoSphere.Web.Client.Extensions;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Http.Resilience;
using R3;
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
builder.Services.AddBlazorWebAssemblyR3();
builder.Services.AddRealtimeNotificationsClient();

var apiUri = new Uri(new Uri(builder.HostEnvironment.BaseAddress), "api");
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

builder.Services.Configure<RealtimeNotificationsClientSettings>(settings =>
{
	settings.Url = new Uri(new Uri(builder.HostEnvironment.BaseAddress), "realtimeNotifications");
});

builder.Services.AddScoped<AuthenticationStateProvider, UserProfileAuthenticationStateProvider>();

var host = builder.Build();

await host.RunAsync();