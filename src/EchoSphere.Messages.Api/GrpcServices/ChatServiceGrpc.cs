using EchoSphere.Domain.Abstractions.Extensions;
using EchoSphere.Domain.Abstractions.Models;
using EchoSphere.GrpcShared.Contracts;
using EchoSphere.GrpcShared.Errors;
using EchoSphere.GrpcShared.Extensions;
using EchoSphere.Messages.Abstractions;
using EchoSphere.Messages.Grpc;
using EchoSphere.Users.Abstractions.Extensions;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;

namespace EchoSphere.Messages.Api.GrpcServices;

[Authorize]
internal sealed class ChatServiceGrpc : Grpc.ChatService.ChatServiceBase
{
	private readonly IChatService _chatService;

	public ChatServiceGrpc(IChatService chatService)
	{
		_chatService = chatService;
	}

	public override Task<GetUserChatsResponse> GetCurrentUserChats(Empty request, ServerCallContext context) =>
		_chatService.GetCurrentUserChats(context.CancellationToken)
			.MapAsync(chats => new GetUserChatsResponse
			{
				Chats =
				{
					chats.Select(x => new ChatInfoDto
					{
						Id = x.Id.ToInnerString(),
						Participants =
						{
							x.Participants.Select(y => y.ToInnerString()),
						},
					}),
				},
			})
			.IfNone(() => throw NotFoundError.Instance.ToStatusRpcException());

	public override Task<GetChatMessagesResponse> GetChatMessages(ChatIdDto request, ServerCallContext context) =>
		_chatService.GetChatMessages(request.ToModel(), context.CancellationToken)
			.MapAsync(messages => new GetChatMessagesResponse
			{
				Messages =
				{
					messages.Select(x => new ChatMessageDto
					{
						Id = x.Id.Value,
						Timestamp = Timestamp.FromDateTimeOffset(x.Timestamp),
						SenderId = x.SenderId.ToInnerString(),
						Text = x.Text,
					}),
				},
			})
			.IfNone(() => throw NotFoundError.Instance.ToStatusRpcException());

	public override Task<ChatIdDto> CreateChat(CreateChatRequest request, ServerCallContext context) =>
		_chatService
			.CreateChat(
				request.Participants.Select(IdValueExtensions.Parse<UserId>).ToArray(), context.CancellationToken)
			.MapAsync(chatId => chatId.ToDto())
			.IfLeft(err => throw err.Match(
				userId => new CreateChatErrorDto
				{
					ParticipantNotFound = userId.ToInnerString(),
				}.ToStatusRpcException()));

	public override Task<MessageIdDto> SendMessage(SendMessageRequest request, ServerCallContext context) =>
		_chatService
			.SendMessage(IdValueExtensions.Parse<ChatId>(request.ChatId), request.Text, context.CancellationToken)
			.MapAsync(messageId => messageId.ToDto())
			.IfLeft(err => throw err.Match(
				() => new SendMessageErrorDto
				{
					ChatNotFound = GrpcExtensions.EmptyInstance,
				}).ToStatusRpcException());
}