using EchoSphere.Domain.Abstractions.Models;
using EchoSphere.Messages.Abstractions;
using EchoSphere.Messages.Abstractions.IntegrationEvents;

namespace EchoSphere.RealtimeNotifications.Api.EventUsersProviders;

internal sealed class MessageSentEventUsersProvider : IEventUsersProvider<MessageSentEvent>
{
	private readonly IChatService _chatService;

	public MessageSentEventUsersProvider(IChatService chatService)
	{
		_chatService = chatService;
	}

	public async ValueTask<IEnumerable<UserId>> GetEventUsers(MessageSentEvent @event, CancellationToken cancellationToken)
	{
		var chatOption = await _chatService.GetChat(@event.ChatId, cancellationToken);
		return chatOption
			.Map(chat => chat.Participants.Where(userId => userId != @event.SenderId))
			.IfNone([]);
	}
}