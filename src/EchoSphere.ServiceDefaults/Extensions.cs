using System.Text.Json;
using EchoSphere.Domain.Abstractions.Extensions;
using EchoSphere.Infrastructure.IntegrationEvents.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using ZiggyCreatures.Caching.Fusion;

namespace EchoSphere.ServiceDefaults;

public static class Extensions
{
	public static IHostApplicationBuilder AddServiceDefaults(this IHostApplicationBuilder builder)
	{
		builder.ConfigureOpenTelemetry();

		builder.AddDefaultHealthChecks();

		builder.Services.AddServiceDiscovery();

		builder.Services.ConfigureHttpClientDefaults(http =>
		{
			http.AddStandardResilienceHandler().Configure(opt =>
			{
				opt.AttemptTimeout.Timeout = TimeSpan.FromSeconds(30);
				opt.CircuitBreaker.SamplingDuration = opt.AttemptTimeout.Timeout * 2;
			});

			http.AddServiceDiscovery();
		});

		builder.AddRedisDistributedCache("cache");

		builder.Services.AddFusionCache()
			.WithSystemTextJsonSerializer(new JsonSerializerOptions(JsonSerializerDefaults.General).AddDomainConverters())
			.WithDefaultEntryOptions(opt => opt
				.SetDuration(TimeSpan.FromSeconds(30))
				.SetDistributedCacheDuration(TimeSpan.FromMinutes(5)))
			.WithRegisteredDistributedCache()
			.WithoutBackplane();

		return builder;
	}

	public static IHostApplicationBuilder ConfigureOpenTelemetry(this IHostApplicationBuilder builder)
	{
		builder.Logging.AddOpenTelemetry(logging =>
		{
			logging.IncludeFormattedMessage = true;
			logging.IncludeScopes = true;
		});

		builder.Services.AddOpenTelemetry()
			.WithMetrics(metrics =>
			{
				metrics.AddAspNetCoreInstrumentation()
					.AddHttpClientInstrumentation()
					.AddRuntimeInstrumentation()
					.AddFusionCacheInstrumentation(opt =>
					{
						opt.IncludeMemoryLevel = true;
						opt.IncludeDistributedLevel = true;
					});
			})
			.WithTracing(tracing =>
			{
				if (builder.Environment.IsDevelopment())
				{
					// We want to view all traces in development
					tracing.SetSampler(new AlwaysOnSampler());
				}

				tracing.AddAspNetCoreInstrumentation()
					.AddGrpcClientInstrumentation()
					.AddHttpClientInstrumentation()
					.AddFusionCacheInstrumentation()
					.AddIntegrationEventInstrumentation();
			});

		builder.AddOpenTelemetryExporters();

		return builder;
	}

	private static IHostApplicationBuilder AddOpenTelemetryExporters(this IHostApplicationBuilder builder)
	{
		var useOtlpExporter = !string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);

		if (useOtlpExporter)
		{
			builder.Services.Configure<OpenTelemetryLoggerOptions>(logging => logging.AddOtlpExporter());
			builder.Services.ConfigureOpenTelemetryMeterProvider(metrics => metrics.AddOtlpExporter());
			builder.Services.ConfigureOpenTelemetryTracerProvider(tracing => tracing.AddOtlpExporter());
		}

		// Uncomment the following lines to enable the Prometheus exporter (requires the OpenTelemetry.Exporter.Prometheus.AspNetCore package)
		// builder.Services.AddOpenTelemetry()
		//    .WithMetrics(metrics => metrics.AddPrometheusExporter());

		// Uncomment the following lines to enable the Azure Monitor exporter (requires the Azure.Monitor.OpenTelemetry.AspNetCore package)
		// if (!string.IsNullOrEmpty(builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]))
		// {
		//    builder.Services.AddOpenTelemetry()
		//       .UseAzureMonitor();
		// }

		return builder;
	}

	public static IHostApplicationBuilder AddDefaultHealthChecks(this IHostApplicationBuilder builder)
	{
		builder.Services.AddHealthChecks()
			// Add a default liveness check to ensure app is responsive
			.AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

		return builder;
	}

	public static WebApplication MapDefaultEndpoints(this WebApplication app)
	{
		// Uncomment the following line to enable the Prometheus endpoint (requires the OpenTelemetry.Exporter.Prometheus.AspNetCore package)
		// app.MapPrometheusScrapingEndpoint();

		// Adding health checks endpoints to applications in non-development environments has security implications.
		// See https://aka.ms/dotnet/aspire/healthchecks for details before enabling these endpoints in non-development environments.
		if (app.Environment.IsDevelopment())
		{
			// All health checks must pass for app to be considered ready to accept traffic after starting
			app.MapHealthChecks("/health");

			// Only health checks tagged with the "live" tag must pass for app to be considered alive
			app.MapHealthChecks("/alive", new HealthCheckOptions
			{
				Predicate = r => r.Tags.Contains("live"),
			});
		}

		return app;
	}
}