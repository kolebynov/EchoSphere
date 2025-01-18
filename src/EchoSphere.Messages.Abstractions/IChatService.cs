using Dusharp;
using EchoSphere.Domain.Abstractions.Models;
using EchoSphere.Messages.Abstractions.Models;
using LanguageExt;

namespace EchoSphere.Messages.Abstractions;

[Union]
public partial struct CreateChatError
{
	[UnionCase]
	public static partial CreateChatError ParticipantUserNotFound(UserId userId);
}

[Union]
public partial struct SendMessageError
{
	[UnionCase]
	public static partial SendMessageError ChatNotFound();
}

public interface IChatService
{
	Task<Option<IReadOnlyList<ChatInfo>>> GetCurrentUserChats(CancellationToken cancellationToken);

	Task<Option<ChatInfo>> GetChat(ChatId chatId, CancellationToken cancellationToken);

	Task<Option<IReadOnlyList<ChatMessage>>> GetChatMessages(ChatId chatId, CancellationToken cancellationToken);

	Task<Either<CreateChatError, ChatId>> CreateChat(
		IReadOnlyList<UserId> participants, CancellationToken cancellationToken);

	Task<Either<SendMessageError, MessageId>> SendMessage(ChatId chatId, string text,
		CancellationToken cancellationToken);
}