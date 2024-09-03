using EchoSphere.UserMessageService;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire components.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();

builder.Services.AddGrpcClient<MessageService.MessageServiceClient>(o => o.Address = new("http://usermessages"));

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

app.MapGet(
	"/messages",
	async ([FromQuery] string fromUserId, [FromQuery] string toUserId, MessageService.MessageServiceClient userMessageClient) =>
	{
		var response = await userMessageClient.GetUserMessagesAsync(
			new GetUserMessagesRequest { FromUserId = fromUserId, ToUserId = toUserId });
		return response.Messages
			.Select(x => x.Text);
	});

app.MapPost(
	"/messages",
	async ([FromBody] SendMessageRequest request, MessageService.MessageServiceClient userMessageClient) =>
	{
		await userMessageClient.SendMessageAsync(request);
	});

app.MapDefaultEndpoints();

app.Run();