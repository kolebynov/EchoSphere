using EchoSphere.Messages.Abstractions;
using EchoSphere.Messages.Abstractions.Models;
using EchoSphere.Messages.Api.Grpc;
using EchoSphere.Users.Abstractions.Models;

namespace EchoSphere.Messages.Client;

public sealed class ChatService : IChatService
{
	private readonly ChatServiceGrpc.ChatServiceGrpcClient _serviceGrpcClient;

	public ChatService(ChatServiceGrpc.ChatServiceGrpcClient serviceGrpcClient)
	{
		_serviceGrpcClient = serviceGrpcClient;
	}

	public async ValueTask<IReadOnlyList<ChatInfo>> GetUserChats(UserId userId, CancellationToken cancellationToken)
	{
		var response = await _serviceGrpcClient.GetUserChatsAsync(
			new UserIdDto { Value = userId.Value.ToString() },
			cancellationToken: cancellationToken);
		return response.Chats
			.Select(x => new ChatInfo
			{
				Id = new ChatId(Guid.Parse(x.Id)),
				Participants = x.Participants.Select(y => new UserId(Guid.Parse(y))).ToArray(),
			})
			.ToArray();
	}

	public async ValueTask<IReadOnlyList<ChatMessage>> GetChatMessages(ChatId chatId, CancellationToken cancellationToken)
	{
		var response = await _serviceGrpcClient.GetChatMessagesAsync(
			new ChatIdDto { Value = chatId.Value.ToString() }, cancellationToken: cancellationToken);
		return response.Messages
			.Select(x => new ChatMessage
			{
				Id = new MessageId(x.Id),
				Timestamp = x.Timestamp.ToDateTimeOffset(),
				SenderId = new UserId(Guid.Parse(x.SenderId)),
				Text = x.Text,
			})
			.ToArray();
	}

	public async ValueTask CreateChat(IReadOnlyList<UserId> participants, CancellationToken cancellationToken)
	{
		await _serviceGrpcClient.CreateChatAsync(
			new CreateChatRequest
			{
				Participants = { participants.Select(x => x.Value.ToString()) },
			},
			cancellationToken: cancellationToken);
	}

	public async ValueTask SendMessage(ChatId chatId, UserId senderId, string text, CancellationToken cancellationToken)
	{
		await _serviceGrpcClient.SendMessageAsync(
			new SendMessageRequest
			{
				ChatId = chatId.Value.ToString(),
				SenderId = senderId.Value.ToString(),
				Text = text,
			},
			cancellationToken: cancellationToken);
	}
}