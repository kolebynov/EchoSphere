using System.Runtime.Versioning;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using Microsoft.Extensions.DependencyInjection;

namespace EchoSphere.BlazorShared.Extensions;

[SupportedOSPlatform("browser")]
public static class HttpClientBuilderExtensions
{
	public static IHttpClientBuilder IncludeBrowserCredentials(this IHttpClientBuilder builder)
	{
		return builder.AddHttpMessageHandler(() => new IncludeBrowserCredentialsMessageHandler());
	}

	private sealed class IncludeBrowserCredentialsMessageHandler : DelegatingHandler
	{
		protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
			return base.Send(request, cancellationToken);
		}

		protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
			return base.SendAsync(request, cancellationToken);
		}
	}
}