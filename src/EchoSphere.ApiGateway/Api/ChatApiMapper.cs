using System.Security.Claims;
using EchoSphere.ApiGateway.Contracts;
using EchoSphere.ApiGateway.Extensions;
using EchoSphere.Messages.Abstractions;
using EchoSphere.Messages.Abstractions.Models;
using EchoSphere.Users.Abstractions.Models;
using Microsoft.AspNetCore.Mvc;

namespace EchoSphere.ApiGateway.Api;

public static class ChatApiMapper
{
	public static IEndpointRouteBuilder MapChatApi(this IEndpointRouteBuilder routeBuilder)
	{
		var chatApi = routeBuilder.MapGroup("/chats").RequireAuthorization();
		chatApi.MapGet(
			"/",
			async (IChatService chatService, ClaimsPrincipal user, CancellationToken cancellationToken) =>
			{
				var currentUserId = user.GetUserId();
				var chats = await chatService.GetUserChats(currentUserId, cancellationToken);
				return chats
					.Select(x => new ChatInfoDtoV1
					{
						Id = x.Id.Value,
						Participants = x.Participants.Select(y => y.Value).ToArray(),
					});
			});

		chatApi.MapPost(
			"/",
			async (IChatService chatService, ClaimsPrincipal user, [FromBody] CreateChatRequestV1 request,
				CancellationToken cancellationToken) =>
			{
				var currentUserId = user.GetUserId();
				await chatService.CreateChat(
					request.Participants.Select(x => new UserId(x)).Append(currentUserId).ToArray(),
					cancellationToken);
			});

		chatApi.MapGet(
			"/{chatId:guid}/messages",
			async (IChatService chatService, Guid chatId, CancellationToken cancellationToken) =>
			{
				var messages = await chatService.GetChatMessages(new ChatId(chatId), cancellationToken);
				return messages
					.Select(x => new ChatMessageDtoV1
					{
						Id = x.Id.Value,
						Timestamp = x.Timestamp,
						SenderId = x.SenderId.Value,
						Text = x.Text,
					});
			});

		chatApi.MapPost(
			"/{chatId:guid}/messages",
			async (IChatService chatService, ClaimsPrincipal user, Guid chatId, [FromBody] SendMessageRequestV1 request,
				CancellationToken cancellationToken) =>
			{
				var currentUserId = user.GetUserId();
				await chatService.SendMessage(new ChatId(chatId), currentUserId, request.Text, cancellationToken);
			});

		return routeBuilder;
	}
}