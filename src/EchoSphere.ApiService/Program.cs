using EchoSphere.UserMessagesApi.Grpc;
using EchoSphere.UsersApi.Grpc;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddProblemDetails();

builder.Services.AddGrpcClient<MessagesService.MessagesServiceClient>(o => o.Address = new("http://UserMessagesApi"));
builder.Services.AddGrpcClient<UsersService.UsersServiceClient>(o => o.Address = new("http://UsersApi"));

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

app.MapGet(
	"/messages",
	async ([FromQuery] string fromUserId, [FromQuery] string toUserId, MessagesService.MessagesServiceClient userMessageClient) =>
	{
		var response = await userMessageClient.GetUserMessagesAsync(
			new GetUserMessagesRequest { FromUserId = fromUserId, ToUserId = toUserId });
		return response.Messages.Select(x => x.Text);
	});

app.MapPost(
	"/messages",
	async ([FromBody] SendMessageRequest request, MessagesService.MessagesServiceClient userMessageClient) =>
	{
		await userMessageClient.SendMessageAsync(request);
	});

app.MapGet(
	"/users",
	async (UsersService.UsersServiceClient usersClient) =>
	{
		var response = await usersClient.GetUserProfilesAsync(new GetUserProfilesRequest());
		return response.Profiles;
	});

app.MapPost(
	"/users",
	async ([FromBody] CreateUserRequest request, UsersService.UsersServiceClient usersClient) => await usersClient.CreateUserAsync(request));

app.MapDefaultEndpoints();

app.Run();