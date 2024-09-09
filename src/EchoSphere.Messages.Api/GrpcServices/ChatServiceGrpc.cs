using EchoSphere.GrpcModels;
using EchoSphere.Messages.Abstractions;
using EchoSphere.Messages.Abstractions.Models;
using EchoSphere.Messages.Grpc;
using EchoSphere.Users.Abstractions.Models;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace EchoSphere.Messages.Api.GrpcServices;

internal sealed class ChatServiceGrpc : Grpc.ChatService.ChatServiceBase
{
	private readonly IChatService _chatService;

	public ChatServiceGrpc(IChatService chatService)
	{
		_chatService = chatService;
	}

	public override async Task<GetUserChatsResponse> GetUserChats(UserIdDto request, ServerCallContext context)
	{
		var chats = await _chatService.GetUserChats(new UserId(Guid.Parse(request.Value)), context.CancellationToken);
		var response = new GetUserChatsResponse();
		response.Chats.AddRange(chats.Select(x =>
		{
			var chatInfoDto = new ChatInfoDto
			{
				Id = x.Id.Value.ToString(),
			};
			chatInfoDto.Participants.AddRange(x.Participants.Select(y => y.Value.ToString()));
			return chatInfoDto;
		}));

		return response;
	}

	public override async Task<GetChatMessagesResponse> GetChatMessages(ChatIdDto request, ServerCallContext context)
	{
		var messages = await _chatService.GetChatMessages(new ChatId(Guid.Parse(request.Value)), context.CancellationToken);
		var response = new GetChatMessagesResponse();
		response.Messages.AddRange(messages.Select(x => new ChatMessageDto
		{
			Id = x.Id.Value,
			Timestamp = Timestamp.FromDateTimeOffset(x.Timestamp),
			SenderId = x.SenderId.Value.ToString(),
			Text = x.Text,
		}));
		return response;
	}

	public override async Task<ChatIdDto> CreateChat(CreateChatRequest request, ServerCallContext context)
	{
		var chatId = await _chatService.CreateChat(
			request.Participants.Select(x => new UserId(Guid.Parse(x))).ToArray(), context.CancellationToken);
		return new ChatIdDto { Value = chatId.Value.ToString() };
	}

	public override async Task<Empty> SendMessage(SendMessageRequest request, ServerCallContext context)
	{
		await _chatService.SendMessage(new ChatId(Guid.Parse(request.ChatId)), new UserId(Guid.Parse(request.SenderId)), request.Text, context.CancellationToken);
		return new Empty();
	}
}