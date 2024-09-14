using EchoSphere.GrpcModels;
using EchoSphere.Messages.Abstractions;
using EchoSphere.Messages.Abstractions.Models;
using EchoSphere.Messages.Grpc;
using EchoSphere.SharedModels.Extensions;
using EchoSphere.Users.Abstractions.Models;

namespace EchoSphere.Messages.Client;

public sealed class ChatGrpcClient : IChatService
{
	private readonly ChatService.ChatServiceClient _serviceGrpcClient;

	public ChatGrpcClient(ChatService.ChatServiceClient serviceGrpcClient)
	{
		_serviceGrpcClient = serviceGrpcClient;
	}

	public async ValueTask<IReadOnlyList<ChatInfo>> GetUserChats(UserId userId, CancellationToken cancellationToken)
	{
		var response = await _serviceGrpcClient.GetUserChatsAsync(
			new UserIdDto { Value = userId.ToInnerString() },
			cancellationToken: cancellationToken);
		return response.Chats
			.Select(x => new ChatInfo
			{
				Id = IdValueExtensions.Parse<ChatId>(x.Id),
				Participants = x.Participants.Select(y => IdValueExtensions.Parse<UserId>(y)).ToArray(),
			})
			.ToArray();
	}

	public async ValueTask<IReadOnlyList<ChatMessage>> GetChatMessages(ChatId chatId, CancellationToken cancellationToken)
	{
		var response = await _serviceGrpcClient.GetChatMessagesAsync(
			new ChatIdDto { Value = chatId.ToInnerString() }, cancellationToken: cancellationToken);
		return response.Messages
			.Select(x => new ChatMessage
			{
				Id = new MessageId(x.Id),
				Timestamp = x.Timestamp.ToDateTimeOffset(),
				SenderId = IdValueExtensions.Parse<UserId>(x.SenderId),
				Text = x.Text,
			})
			.ToArray();
	}

	public async ValueTask<ChatId> CreateChat(IReadOnlyList<UserId> participants, CancellationToken cancellationToken)
	{
		var chatId = await _serviceGrpcClient.CreateChatAsync(
			new CreateChatRequest
			{
				Participants = { participants.Select(x => x.ToInnerString()) },
			},
			cancellationToken: cancellationToken);
		return IdValueExtensions.Parse<ChatId>(chatId.Value);
	}

	public async ValueTask SendMessage(ChatId chatId, UserId senderId, string text, CancellationToken cancellationToken)
	{
		await _serviceGrpcClient.SendMessageAsync(
			new SendMessageRequest
			{
				ChatId = chatId.ToInnerString(),
				SenderId = senderId.ToInnerString(),
				Text = text,
			},
			cancellationToken: cancellationToken);
	}
}