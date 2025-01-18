using EchoSphere.Domain.AspNetCore.Extensions;
using EchoSphere.Infrastructure.IntegrationEvents.Abstractions;
using EchoSphere.Infrastructure.IntegrationEvents.Extensions;
using EchoSphere.Messages.Abstractions.IntegrationEvents;
using EchoSphere.Messages.Client.Extensions;
using EchoSphere.RealtimeNotifications.Api;
using EchoSphere.ServiceDefaults;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults().AddIntegrationEvents();

builder.Services.AddAsyncInitialization();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
	.AddJwtBearer(opt =>
	{
		opt.RequireHttpsMetadata = true;
		opt.TokenValidationParameters = new TokenValidationParameters
		{
#pragma warning disable CA5404
			ValidateIssuer = false,
			// ValidIssuer = JwtOptions.Issuer,
			ValidateAudience = false,
#pragma warning restore CA5404
			// ValidAudience = JwtOptions.Audience,
			ValidateLifetime = true,
			IssuerSigningKey = new JsonWebKey(File.ReadAllText(Path.Combine("..", "EchoSphere.Accounts.WebApp", "tempkey.jwk"))),
			ValidateIssuerSigningKey = true,
		};
	});
builder.Services.AddAuthorization();

builder.Services.AddSignalR();

builder.Services.AddDomainServicesCore();

builder.Services.AddMessagesGrpcClient(new Uri("https://MessagesApi"));

builder.Services.AddScoped<IIntegrationEventHandler<MessageSentEvent>, MessagesSentHandler>();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapHub<NotificationsHub>("/realtimeNotifications");

await app.InitAndRunAsync();