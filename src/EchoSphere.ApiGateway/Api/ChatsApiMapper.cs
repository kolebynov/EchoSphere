using EchoSphere.ApiGateway.Contracts;
using EchoSphere.Domain.Abstractions.Models;
using EchoSphere.Messages.Abstractions;
using EchoSphere.Users.Abstractions.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace EchoSphere.ApiGateway.Api;

public static class ChatsApiMapper
{
	public static IEndpointRouteBuilder MapChatsApi(this IEndpointRouteBuilder routeBuilder)
	{
		var chatsApi = routeBuilder.MapGroup("/chats").RequireAuthorization();
		chatsApi.MapGet(
			"/",
			(IChatService chatService, CancellationToken cancellationToken) =>
			{
				return chatService.GetCurrentUserChats(cancellationToken)
					.MapAsync(chats => Results.Ok(chats
						.Select(x => new ChatInfoDtoV1
						{
							Id = x.Id.Value,
							Participants = x.Participants.Select(y => y.Value).ToArray(),
						})))
					.IfNone(() => Results.Problem(statusCode: 404));
			});

		chatsApi.MapPost(
			"/",
			(IChatService chatService, [FromBody] CreateChatRequestV1 request,
				CancellationToken cancellationToken) =>
			{
				return chatService
					.CreateChat(request.Participants.Select(x => new UserId(x)).ToArray(), cancellationToken)
					.MapAsync(chatId => Results.Ok(chatId.Value))
					.IfLeft(err => err.Match(userId => Results.Problem(statusCode: 400, type: "participant_not_found", detail: $"Participant {userId} not found")));
			});

		chatsApi.MapGet(
			"/{chatId:guid}/messages",
			(IChatService chatService, Guid chatId, CancellationToken cancellationToken) =>
			{
				return chatService.GetChatMessages(new ChatId(chatId), cancellationToken)
					.MapAsync(messages => Results.Ok(messages
						.Select(x => new ChatMessageDtoV1
						{
							Id = x.Id.Value,
							Timestamp = x.Timestamp,
							SenderId = x.SenderId.Value,
							Text = x.Text,
						})))
					.IfNone(() => Results.Problem(statusCode: 404));
			});

		chatsApi.MapPost(
			"/{chatId:guid}/messages",
			(IChatService chatService, Guid chatId, [FromBody] SendMessageRequestV1 request,
				CancellationToken cancellationToken) =>
			{
				return chatService.SendMessage(new ChatId(chatId), request.Text, cancellationToken)
					.MapAsync(messageId => Results.Ok(messageId.Value))
					.IfLeft(err => err.Match(
						() => Results.Problem(statusCode: 404)));
			});

		return routeBuilder;
	}
}