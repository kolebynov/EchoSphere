using EchoSphere.Domain.Abstractions.Extensions;
using EchoSphere.Infrastructure.IntegrationEvents.Abstractions;
using EchoSphere.Messages.Abstractions;
using EchoSphere.Messages.Abstractions.IntegrationEvents;
using Microsoft.AspNetCore.SignalR;

namespace EchoSphere.RealtimeNotifications.Api;

internal sealed class MessagesSentHandler : IIntegrationEventHandler<MessageSentEvent>
{
	private readonly IHubContext<NotificationsHub> _hubContext;
	private readonly IChatService _chatService;

	public MessagesSentHandler(IHubContext<NotificationsHub> hubContext, IChatService chatService)
	{
		_hubContext = hubContext;
		_chatService = chatService;
	}

	public async ValueTask Handle(MessageSentEvent @event, CancellationToken cancellationToken)
	{
		var chatOption = await _chatService.GetChat(@event.ChatId, cancellationToken);
		var task = chatOption
			.Map(chat =>
			{
				var participants = chat.Participants
					.Where(userId => userId != @event.SenderId)
					.Select(userId => userId.ToInnerString());
				return _hubContext.Clients.Users(participants).SendAsync("MessageSent", @event, cancellationToken);
			})
			.IfNone(Task.CompletedTask);
		await task;
	}
}