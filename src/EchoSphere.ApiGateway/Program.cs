using EchoSphere.ApiGateway.Api;
using EchoSphere.Messages.Client.Extensions;
using EchoSphere.Posts.Client.Extensions;
using EchoSphere.ServiceDefaults;
using EchoSphere.Users.Client.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
	.AddJwtBearer(opt =>
	{
		opt.RequireHttpsMetadata = true;
		opt.TokenValidationParameters = new TokenValidationParameters
		{
			ValidateIssuer = false,
			// ValidIssuer = JwtOptions.Issuer,
			ValidateAudience = false,
			// ValidAudience = JwtOptions.Audience,
			ValidateLifetime = true,
			IssuerSigningKey = new JsonWebKey(File.ReadAllText(Path.Combine("..", "EchoSphere.Accounts.WebApp", "tempkey.jwk"))),
			ValidateIssuerSigningKey = true,
		};
	});
builder.Services.AddAuthorization();

builder.Services.AddHttpContextAccessor();

builder.Services.AddProblemDetails();

builder.Services.AddUsersGrpcClient(new Uri("https://UsersApi"));
builder.Services.AddMessagesGrpcClient(new Uri("https://MessagesApi"));
builder.Services.AddPostsGrpcClient(new Uri("https://PostsApi"));

var app = builder.Build();

app.UseExceptionHandler();

app.UseAuthentication();
app.UseAuthorization();

app.MapApi();

app.MapDefaultEndpoints();

app.Run();