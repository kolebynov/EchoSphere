using EchoSphere.Messages.Abstractions;
using EchoSphere.Messages.Abstractions.Models;
using EchoSphere.Messages.Api.Data.Models;
using EchoSphere.Users.Abstractions.Models;
using LinqToDB;
using LinqToDB.Data;

namespace EchoSphere.Messages.Api.Services;

internal sealed class ChatService : IChatService
{
	private readonly IDataContext _dataContext;

	public ChatService(IDataContext dataContext)
	{
		_dataContext = dataContext;
	}

	public async ValueTask<IReadOnlyList<ChatInfo>> GetUserChats(UserId userId, CancellationToken cancellationToken)
	{
		var chatParticipants = _dataContext.GetTable<ChatParticipantDb>();

		var chatIds = chatParticipants
			.Where(x => x.UserId == userId)
			.Select(x => x.ChatId);

		var asyncEnumerable = chatParticipants
			.Where(x => chatIds.Contains(x.ChatId))
			.AsAsyncEnumerable();
		return await asyncEnumerable
			.GroupBy(x => x.ChatId)
			.SelectAwait(async groupedChat => new ChatInfo
			{
				Id = groupedChat.Key,
				Participants = await groupedChat.Select(x => x.UserId).ToArrayAsync(cancellationToken),
			})
			.ToArrayAsync(cancellationToken);
	}

	public async ValueTask<IReadOnlyList<ChatMessage>> GetChatMessages(ChatId chatId, CancellationToken cancellationToken)
	{
		return await _dataContext.GetTable<ChatMessageDb>()
			.Where(x => x.ChatId == chatId)
			.Select(x => new ChatMessage
			{
				Id = x.Id,
				Timestamp = x.Timestamp,
				SenderId = x.SenderId,
				Text = x.Text,
			})
			.ToArrayAsync(cancellationToken);
	}

	public async ValueTask<ChatId> CreateChat(IReadOnlyList<UserId> participants, CancellationToken cancellationToken)
	{
		var chatId = new ChatId(Guid.NewGuid());
		await _dataContext.GetTable<ChatParticipantDb>()
			.BulkCopyAsync(
				participants.Select(userId => new ChatParticipantDb
				{
					ChatId = chatId,
					UserId = userId,
				}),
				cancellationToken);

		return chatId;
	}

	public async ValueTask SendMessage(ChatId chatId, UserId senderId, string text, CancellationToken cancellationToken)
	{
		await _dataContext.InsertAsync(
			new ChatMessageDb
			{
				Timestamp = DateTimeOffset.UtcNow,
				ChatId = chatId,
				SenderId = senderId,
				Text = text,
			}, token: cancellationToken);
	}
}