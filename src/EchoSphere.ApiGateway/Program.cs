using EchoSphere.ApiGateway.Api;
using EchoSphere.Messages.Abstractions;
using EchoSphere.Messages.Client;
using EchoSphere.Messages.Grpc;
using EchoSphere.Posts.Abstractions;
using EchoSphere.Posts.Client;
using EchoSphere.Posts.Grpc;
using EchoSphere.ServiceDefaults;
using EchoSphere.Users.Abstractions;
using EchoSphere.Users.Client;
using EchoSphere.Users.Grpc;
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

builder.Services.AddProblemDetails();

builder.Services.AddGrpcClient<ChatService.ChatServiceClient>(o => o.Address = new("http://MessagesApi"));
builder.Services.AddGrpcClient<UserProfileService.UserProfileServiceClient>(o => o.Address = new("http://UsersApi"));
builder.Services.AddGrpcClient<FriendService.FriendServiceClient>(o => o.Address = new("http://UsersApi"));
builder.Services.AddGrpcClient<FollowService.FollowServiceClient>(o => o.Address = new("http://UsersApi"));
builder.Services.AddGrpcClient<PostService.PostServiceClient>(o => o.Address = new("http://PostsApi"));

builder.Services.AddScoped<IChatService, ChatGrpcClient>();
builder.Services.AddScoped<IUserProfileService, UserProfileGrpcClient>();
builder.Services.AddScoped<IFriendService, FriendGrpcClient>();
builder.Services.AddScoped<IFollowService, FollowGrpcClient>();
builder.Services.AddScoped<IPostService, PostGrpcClient>();

var app = builder.Build();

app.UseExceptionHandler();

app.UseAuthentication();
app.UseAuthorization();

app.MapApi();

app.MapDefaultEndpoints();

app.Run();