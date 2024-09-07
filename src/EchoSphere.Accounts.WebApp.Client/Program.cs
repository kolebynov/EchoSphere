using EchoSphere.Accounts.WebApp.Client.Extensions;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection.Extensions;

if (!OperatingSystem.IsBrowser())
{
	return;
}

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddCommonClientAndServerServices();

builder.Services.TryAddScoped(_ => new HttpClient
{
	BaseAddress = new Uri(new Uri(builder.HostEnvironment.BaseAddress), "api/"),
	Timeout = TimeSpan.FromSeconds(60),
});

var host = builder.Build();

await host.RunAsync();