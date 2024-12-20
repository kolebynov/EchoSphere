using EchoSphere.ApiGateway.Api;
using EchoSphere.Domain.Abstractions.Extensions;
using EchoSphere.Messages.Client.Extensions;
using EchoSphere.Notifications.Client.Extensions;
using EchoSphere.Posts.Client.Extensions;
using EchoSphere.ServiceDefaults;
using EchoSphere.Users.Client.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

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

		opt.Events = new JwtBearerEvents
		{
			OnMessageReceived = context =>
			{
				var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
				if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.Ordinal))
				{
					context.Token = authHeader["Bearer ".Length..].Trim();
				}

				if (string.IsNullOrEmpty(context.Token))
				{
					context.Token = context.Request.Cookies["AccessToken"];
				}

				return Task.CompletedTask;
			},
		};
	});
builder.Services.AddAuthorization();

builder.Services.AddHttpContextAccessor();

builder.Services.AddProblemDetails();

builder.Services.Configure<JsonOptions>(opt => opt.SerializerOptions.AddIdValueConverters());

builder.Services.AddUsersGrpcClient(new Uri("https://UsersApi"));
builder.Services.AddMessagesGrpcClient(new Uri("https://MessagesApi"));
builder.Services.AddPostsGrpcClient(new Uri("https://PostsApi"));
builder.Services.AddNotificationsGrpcClient(new Uri("https://NotificationsApi"));

var app = builder.Build();

app.UseExceptionHandler();

app.UseAuthentication();
app.UseAuthorization();

app.MapApi();

app.MapDefaultEndpoints();

app.Run();