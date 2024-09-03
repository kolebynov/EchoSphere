using EchoSphere.UserMessageService;
using EchoSphere.UserMessagesService.Models;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using LinqToDB;

namespace EchoSphere.UserMessagesService.Services;

internal class MessageService : UserMessageService.MessageService.MessageServiceBase
{
	private readonly ILogger<MessageService> _logger;
	private readonly AppDataConnection _appDataConnection;

	public MessageService(ILogger<MessageService> logger, AppDataConnection appDataConnection)
	{
		_logger = logger;
		_appDataConnection = appDataConnection;
	}

	public override async Task<Empty> SendMessage(SendMessageRequest request, ServerCallContext context)
	{
		await _appDataConnection.UserMessages.InsertAsync(
			() => new UserMessage
			{
				FromUserId = Guid.Parse(request.FromUserId),
				ToUserId = Guid.Parse(request.ToUserId),
				Text = request.Text,
			},
			context.CancellationToken);

		return new Empty();
	}

	public override async Task<GetUserMessagesResponse> GetUserMessages(GetUserMessagesRequest request, ServerCallContext context)
	{
		var response = new GetUserMessagesResponse();
		var fromUserId = Guid.Parse(request.FromUserId);
		var toUserId = Guid.Parse(request.ToUserId);
		response.Messages.AddRange(await _appDataConnection.UserMessages
			.Where(x => x.FromUserId == fromUserId && x.ToUserId == toUserId)
			.Select(x => new UserMessageDto { Text = x.Text })
			.ToArrayAsync(context.CancellationToken));
		return response;
	}
}