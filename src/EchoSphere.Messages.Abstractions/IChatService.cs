using EchoSphere.Messages.Abstractions.Models;
using EchoSphere.Users.Abstractions.Models;

namespace EchoSphere.Messages.Abstractions;

public interface IChatService
{
	ValueTask<IReadOnlyList<ChatInfo>> GetUserChats(UserId userId, CancellationToken cancellationToken);

	ValueTask<IReadOnlyList<ChatMessage>> GetChatMessages(ChatId chatId, CancellationToken cancellationToken);

	ValueTask<ChatId> CreateChat(IReadOnlyList<UserId> participants, CancellationToken cancellationToken);

	ValueTask SendMessage(ChatId chatId, UserId senderId, string text, CancellationToken cancellationToken);
}