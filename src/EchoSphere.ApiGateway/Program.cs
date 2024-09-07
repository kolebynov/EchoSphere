using EchoSphere.ApiGateway.Api;
using EchoSphere.Messages.Abstractions;
using EchoSphere.Messages.Api.Grpc;
using EchoSphere.Messages.Client;
using EchoSphere.ServiceDefaults;
using EchoSphere.Users.Api.Grpc;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
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

builder.Services.AddProblemDetails();

builder.Services.AddGrpcClient<ChatServiceGrpc.ChatServiceGrpcClient>(o => o.Address = new("http://MessagesApi"));
builder.Services.AddGrpcClient<UsersService.UsersServiceClient>(o => o.Address = new("http://UsersApi"));

builder.Services.AddScoped<IChatService, ChatService>();

var app = builder.Build();

app.UseExceptionHandler();

app.UseAuthentication();
app.UseAuthorization();

app.MapApi();

app.MapDefaultEndpoints();

app.Run();