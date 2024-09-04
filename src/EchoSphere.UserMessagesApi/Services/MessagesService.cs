using EchoSphere.UserMessagesApi.Grpc;
using EchoSphere.UserMessagesApi.Models;
using Grpc.Core;
using LinqToDB;

namespace EchoSphere.UserMessagesApi.Services;

internal class MessagesService : Grpc.MessagesService.MessagesServiceBase
{
	private readonly ILogger<MessagesService> _logger;
	private readonly AppDataConnection _appDataConnection;

	public MessagesService(ILogger<MessagesService> logger, AppDataConnection appDataConnection)
	{
		_logger = logger;
		_appDataConnection = appDataConnection;
	}

	public override async Task<SendMessageResponse> SendMessage(SendMessageRequest request, ServerCallContext context)
	{
		var messageId = await _appDataConnection.UserMessages.InsertWithInt64IdentityAsync(
			() => new UserMessage
			{
				FromUserId = Guid.Parse(request.FromUserId),
				ToUserId = Guid.Parse(request.ToUserId),
				Text = request.Text,
			},
			context.CancellationToken);

		return new SendMessageResponse { Id = messageId };
	}

	public override async Task<GetUserMessagesResponse> GetUserMessages(
		GetUserMessagesRequest request, ServerCallContext context)
	{
		var response = new GetUserMessagesResponse();
		var fromUserId = Guid.Parse(request.FromUserId);
		var toUserId = Guid.Parse(request.ToUserId);
		response.Messages.AddRange(await _appDataConnection.UserMessages
			.Where(x => x.FromUserId == fromUserId && x.ToUserId == toUserId)
			.Select(x => new UserMessageDto { Id = x.Id, Text = x.Text })
			.ToArrayAsync(context.CancellationToken));
		return response;
	}
}