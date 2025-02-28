using EchoSphere.Domain.Abstractions.Extensions;
using EchoSphere.Domain.AspNetCore.Extensions;
using EchoSphere.Infrastructure.IntegrationEvents.Extensions;
using EchoSphere.Messages.Abstractions.IntegrationEvents;
using EchoSphere.Messages.Client.Extensions;
using EchoSphere.Posts.Abstractions.IntegrationEvents;
using EchoSphere.RealtimeNotifications.Api;
using EchoSphere.RealtimeNotifications.Api.EventUsersProviders;
using EchoSphere.RealtimeNotifications.Api.Extensions;
using EchoSphere.ServiceDefaults;
using EchoSphere.Users.Client.Extensions;
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

builder.Services.AddSignalR()
	.AddJsonProtocol(opt => opt.PayloadSerializerOptions.AddDomainConverters());

builder.Services.AddDomainServicesCore();

builder.Services.AddMessagesGrpcClient(new Uri("https://MessagesApi"));
builder.Services.AddUsersGrpcClient(new Uri("https://UsersApi"));

builder.Services.AddEventToUsersSender<MessageSentEvent, MessageSentEventUsersProvider>();
builder.Services.AddEventToUsersSender<PostPublished, PostPublishedEventUsersProvider>();
builder.Services.AddEventToUsersSender<PostCommentAdded, PostCommentEventUsersProvider>();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapHub<NotificationsHub>("/realtimeNotifications");

await app.InitAndRunAsync();