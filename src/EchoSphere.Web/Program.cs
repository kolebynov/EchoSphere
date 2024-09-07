using EchoSphere.ApiGateway.Client;
using EchoSphere.BlazorShared.Extensions;
using EchoSphere.ServiceDefaults;
using EchoSphere.Web.Components;
using EchoSphere.Web.Extensions;
using Refit;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire components.
builder.AddServiceDefaults();
builder.AddApplicationServices();

// Add services to the container.
builder.Services.AddRazorComponents()
	.AddInteractiveServerComponents();

builder.Services.AddSharedBlazor();

builder.Services.AddOutputCache();

builder.Services.AddRefitClient<IChatClient>()
	.ConfigureHttpClient(client => client.BaseAddress = new("https+http://ApiGateway/api"))
	.AddAccessToken();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Error", createScopeForErrors: true);
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.UseOutputCache();

app.MapRazorComponents<App>()
	.AddInteractiveServerRenderMode();

app.MapDefaultEndpoints();

app.Run();