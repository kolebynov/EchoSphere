using EchoSphere.GrpcModels;
using EchoSphere.Messages.Abstractions;
using EchoSphere.Messages.Abstractions.Models;
using EchoSphere.Messages.Grpc;
using EchoSphere.SharedModels.Extensions;
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
		var chats = await _chatService.GetUserChats(IdValueExtensions.Parse<UserId>(request.Value), context.CancellationToken);
		var response = new GetUserChatsResponse();
		response.Chats.AddRange(chats.Select(x =>
		{
			var chatInfoDto = new ChatInfoDto
			{
				Id = x.Id.ToInnerString(),
			};
			chatInfoDto.Participants.AddRange(x.Participants.Select(y => y.ToInnerString()));
			return chatInfoDto;
		}));

		return response;
	}

	public override async Task<GetChatMessagesResponse> GetChatMessages(ChatIdDto request, ServerCallContext context)
	{
		var messages = await _chatService.GetChatMessages(IdValueExtensions.Parse<ChatId>(request.Value), context.CancellationToken);
		var response = new GetChatMessagesResponse();
		response.Messages.AddRange(messages.Select(x => new ChatMessageDto
		{
			Id = x.Id.Value,
			Timestamp = Timestamp.FromDateTimeOffset(x.Timestamp),
			SenderId = x.SenderId.ToInnerString(),
			Text = x.Text,
		}));
		return response;
	}

	public override async Task<ChatIdDto> CreateChat(CreateChatRequest request, ServerCallContext context)
	{
		var chatId = await _chatService.CreateChat(
			request.Participants.Select(x => IdValueExtensions.Parse<UserId>(x)).ToArray(), context.CancellationToken);
		return new ChatIdDto { Value = chatId.ToInnerString() };
	}

	public override async Task<Empty> SendMessage(SendMessageRequest request, ServerCallContext context)
	{
		await _chatService.SendMessage(IdValueExtensions.Parse<ChatId>(request.ChatId), IdValueExtensions.Parse<UserId>(request.SenderId), request.Text, context.CancellationToken);
		return new Empty();
	}
}