using System.Security.Claims;
using EchoSphere.ApiGateway.Contracts;
using EchoSphere.ApiGateway.Extensions;
using EchoSphere.Messages.Abstractions;
using EchoSphere.Messages.Abstractions.Models;
using EchoSphere.Users.Abstractions.Models;
using Microsoft.AspNetCore.Mvc;

namespace EchoSphere.ApiGateway.Api;

public static class ChatsApiMapper
{
	public static IEndpointRouteBuilder MapChatsApi(this IEndpointRouteBuilder routeBuilder)
	{
		var chatsApi = routeBuilder.MapGroup("/chats").RequireAuthorization();
		chatsApi.MapGet(
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

		chatsApi.MapPost(
			"/",
			async (IChatService chatService, ClaimsPrincipal user, [FromBody] CreateChatRequestV1 request,
				CancellationToken cancellationToken) =>
			{
				var currentUserId = user.GetUserId();
				return (await chatService.CreateChat(
					request.Participants.Select(x => new UserId(x)).Append(currentUserId).ToArray(),
					cancellationToken)).Value;
			});

		chatsApi.MapGet(
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

		chatsApi.MapPost(
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