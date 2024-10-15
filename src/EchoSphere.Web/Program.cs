using EchoSphere.ServiceDefaults;
using EchoSphere.Web.Client.Extensions;
using EchoSphere.Web.Components;
using EchoSphere.Web.Extensions;
using Microsoft.AspNetCore.Authentication;
using Yarp.ReverseProxy.Transforms;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddApplicationServices();

builder.Services.AddRazorComponents()
	.AddInteractiveWebAssemblyComponents()
	.AddInteractiveServerComponents();

builder.Services.AddCommonClientAndServerServices();
builder.Services.AddOutputCache();
builder.Services.AddHttpForwarderWithServiceDiscovery();
builder.Services.AddAsyncInitialization();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Error", createScopeForErrors: true);
	app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.UseOutputCache();

app.MapRazorComponents<App>()
	.AddInteractiveWebAssemblyRenderMode()
	.AddInteractiveServerRenderMode()
	.AddAdditionalAssemblies(typeof(EchoSphere.Web.Client.Components._Imports).Assembly);

app.MapForwarder("/api/{**catch-all}", "https://ApiGateway", transformBuilder =>
{
	transformBuilder.AddRequestTransform(async transformContext =>
	{
		var accessToken = await transformContext.HttpContext.GetTokenAsync("access_token");
		transformContext.ProxyRequest.Headers.Authorization = new("Bearer", accessToken);
	});
}).RequireAuthorization();

await app.InitAndRunAsync();