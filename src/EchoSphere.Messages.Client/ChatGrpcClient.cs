using EchoSphere.Domain.Abstractions.Extensions;
using EchoSphere.Domain.Abstractions.Models;
using EchoSphere.GrpcClientShared;
using EchoSphere.GrpcShared.Errors;
using EchoSphere.GrpcShared.Extensions;
using EchoSphere.Messages.Abstractions;
using EchoSphere.Messages.Abstractions.Models;
using EchoSphere.Messages.Grpc;
using EchoSphere.Users.Abstractions.Extensions;

namespace EchoSphere.Messages.Client;

internal sealed class ChatGrpcClient : IChatService
{
	private readonly GrpcCallExecutor<ChatService.ChatServiceClient> _grpcExecutor;

	public ChatGrpcClient(GrpcCallExecutor<ChatService.ChatServiceClient> grpcExecutor)
	{
		_grpcExecutor = grpcExecutor;
	}

	public Task<Option<IReadOnlyList<ChatInfo>>> GetCurrentUserChats(CancellationToken cancellationToken) =>
		_grpcExecutor
			.ExecuteAsync<Option<IReadOnlyList<ChatInfo>>, NotFoundError>(
				async client =>
				{
					var response = await client.GetCurrentUserChatsAsync(
						GrpcExtensions.EmptyInstance, cancellationToken: cancellationToken);

					var chats = response.Chats
						.Select(x => new ChatInfo
						{
							Id = IdValueExtensions.Parse<ChatId>(x.Id),
							Participants = x.Participants.Select(IdValueExtensions.Parse<UserId>).ToArray(),
						})
						.ToArray();

					return Some<IReadOnlyList<ChatInfo>>(chats);
				})
			.IfLeft(_ => None);

	public Task<Option<IReadOnlyList<ChatMessage>>> GetChatMessages(
		ChatId chatId, CancellationToken cancellationToken) =>
		_grpcExecutor
			.ExecuteAsync<Option<IReadOnlyList<ChatMessage>>, NotFoundError>(
				async client =>
				{
					var response = await client.GetChatMessagesAsync(chatId.ToDto(), cancellationToken: cancellationToken);
					var messages = response.Messages
						.Select(x => new ChatMessage
						{
							Id = new MessageId(x.Id),
							Timestamp = x.Timestamp.ToDateTimeOffset(),
							SenderId = IdValueExtensions.Parse<UserId>(x.SenderId),
							Text = x.Text,
						})
						.ToArray();

					return Some<IReadOnlyList<ChatMessage>>(messages);
				})
			.IfLeft(_ => None);

	public Task<Either<CreateChatError, ChatId>> CreateChat(
		IReadOnlyList<UserId> participants, CancellationToken cancellationToken) =>
		_grpcExecutor
			.ExecuteAsync<ChatId, CreateChatErrorDto>(
				async client =>
				{
					var chatId = await client.CreateChatAsync(
						new CreateChatRequest
						{
							Participants = { participants.Select(x => x.ToInnerString()) },
						},
						cancellationToken: cancellationToken);
					return IdValueExtensions.Parse<ChatId>(chatId.Value);
				})
			.MapLeftAsync(err => err.ErrorCase switch
			{
				CreateChatErrorDto.ErrorOneofCase.ParticipantNotFound => CreateChatError.ParticipantUserNotFound(
					IdValueExtensions.Parse<UserId>(err.ParticipantNotFound)),
				_ => throw new ArgumentOutOfRangeException(nameof(err), err.ErrorCase, null),
			});

	public Task<Either<SendMessageError, MessageId>> SendMessage(ChatId chatId, string text,
		CancellationToken cancellationToken) =>
		_grpcExecutor
			.ExecuteAsync<MessageId, SendMessageErrorDto>(
				async client =>
				{
					var messageId = await client.SendMessageAsync(
						new SendMessageRequest
						{
							ChatId = chatId.ToInnerString(),
							Text = text,
						},
						cancellationToken: cancellationToken);

					return new MessageId(messageId.Value);
				})
			.MapLeftAsync(err => err.ErrorCase switch
			{
				SendMessageErrorDto.ErrorOneofCase.ChatNotFound => SendMessageError.ChatNotFound(),
				_ => throw new ArgumentOutOfRangeException(nameof(err), err.ErrorCase, null),
			});
}