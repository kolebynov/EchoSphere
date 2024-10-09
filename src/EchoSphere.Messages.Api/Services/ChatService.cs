using EchoSphere.Domain.Abstractions;
using EchoSphere.Domain.Abstractions.Models;
using EchoSphere.Messages.Abstractions;
using EchoSphere.Messages.Abstractions.Models;
using EchoSphere.Messages.Api.Data.Models;
using EchoSphere.Users.Abstractions;
using LinqToDB;
using LinqToDB.Data;

namespace EchoSphere.Messages.Api.Services;

internal sealed class ChatService : IChatService
{
	private readonly IDataContext _dataContext;
	private readonly IUserProfileService _userProfileService;
	private readonly ICurrentUserAccessor _currentUserAccessor;

	public ChatService(IDataContext dataContext, IUserProfileService userProfileService,
		ICurrentUserAccessor currentUserAccessor)
	{
		_dataContext = dataContext;
		_userProfileService = userProfileService;
		_currentUserAccessor = currentUserAccessor;
	}

	public async Task<Option<IReadOnlyList<ChatInfo>>> GetCurrentUserChats(CancellationToken cancellationToken)
	{
		var currentUserId = _currentUserAccessor.CurrentUserId;
		var isUserValid = (await _userProfileService.CheckUsersExistence([currentUserId], cancellationToken))[0].Exists;
		if (!isUserValid)
		{
			return None;
		}

		var chatParticipants = _dataContext.GetTable<ChatParticipantDb>();

		var chatIds = chatParticipants
			.Where(x => x.UserId == currentUserId)
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

	public async Task<Option<IReadOnlyList<ChatMessage>>> GetChatMessages(
		ChatId chatId, CancellationToken cancellationToken)
	{
		if (!await IsChatExist(chatId, cancellationToken))
		{
			return None;
		}

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

	public async Task<Either<CreateChatError, ChatId>> CreateChat(
		IReadOnlyList<UserId> participants, CancellationToken cancellationToken)
	{
		var chatId = new ChatId(Guid.NewGuid());
		participants = [..participants, _currentUserAccessor.CurrentUserId];
		var invalidUserId = (await _userProfileService.CheckUsersExistence(participants, cancellationToken))
			.FirstOrDefault(x => !x.Exists).UserId;
		if (invalidUserId != default)
		{
			return CreateChatError.ParticipantUserNotFound(invalidUserId);
		}

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

	public async Task<Either<SendMessageError, MessageId>> SendMessage(ChatId chatId, string text,
		CancellationToken cancellationToken)
	{
		if (!await IsChatExist(chatId, cancellationToken))
		{
			return SendMessageError.ChatNotFound();
		}

		var messageId = await _dataContext.InsertWithInt64IdentityAsync(
			new ChatMessageDb
			{
				Timestamp = DateTimeOffset.UtcNow,
				ChatId = chatId,
				SenderId = _currentUserAccessor.CurrentUserId,
				Text = text,
			}, token: cancellationToken);

		return Right(new MessageId(messageId));
	}

	private Task<bool> IsChatExist(ChatId chatId, CancellationToken cancellationToken) =>
		_dataContext.GetTable<ChatParticipantDb>()
			.AnyAsync(x => x.ChatId == chatId && x.UserId == _currentUserAccessor.CurrentUserId, cancellationToken);
}