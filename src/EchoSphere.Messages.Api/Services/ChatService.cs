using EchoSphere.Domain.Abstractions;
using EchoSphere.Domain.Abstractions.Models;
using EchoSphere.Infrastructure.IntegrationEvents.Abstractions;
using EchoSphere.Messages.Abstractions;
using EchoSphere.Messages.Abstractions.IntegrationEvents;
using EchoSphere.Messages.Abstractions.Models;
using EchoSphere.Messages.Api.Data.Models;
using EchoSphere.Users.Abstractions;
using LinqToDB;
using LinqToDB.Data;

namespace EchoSphere.Messages.Api.Services;

internal sealed class ChatService : IChatService
{
	private readonly DataConnection _dataConnection;
	private readonly IUserProfileService _userProfileService;
	private readonly ICurrentUserAccessor _currentUserAccessor;
	private readonly IIntegrationEventService _integrationEventService;
	private readonly ITable<ChatParticipantDb> _chatParticipantsTable;

	public ChatService(DataConnection dataConnection, IUserProfileService userProfileService,
		ICurrentUserAccessor currentUserAccessor, IIntegrationEventService integrationEventService)
	{
		_dataConnection = dataConnection;
		_userProfileService = userProfileService;
		_currentUserAccessor = currentUserAccessor;
		_integrationEventService = integrationEventService;
		_chatParticipantsTable = _dataConnection.GetTable<ChatParticipantDb>();
	}

	public async Task<Option<IReadOnlyList<ChatInfo>>> GetCurrentUserChats(CancellationToken cancellationToken)
	{
		var currentUserId = _currentUserAccessor.CurrentUserId;
		var isUserValid = (await _userProfileService.CheckUsersExistence([currentUserId], cancellationToken))[0].Exists;
		if (!isUserValid)
		{
			return None;
		}

		var chatIds = _chatParticipantsTable
			.Where(x => x.UserId == currentUserId)
			.Select(x => x.ChatId);

		var asyncEnumerable = _chatParticipantsTable
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

	public async Task<Option<ChatInfo>> GetChat(ChatId chatId, CancellationToken cancellationToken)
	{
		var chatParticipants = await _chatParticipantsTable
			.Where(x => x.ChatId == chatId)
			.Select(x => x.UserId)
			.ToArrayAsync(cancellationToken);

		return chatParticipants.Length > 0 && (_currentUserAccessor.IsSupervisor || chatParticipants.Contains(_currentUserAccessor.CurrentUserId))
			? Some(new ChatInfo { Id = chatId, Participants = chatParticipants })
			: None;
	}

	public async Task<Option<IReadOnlyList<ChatMessage>>> GetChatMessages(
		ChatId chatId, CancellationToken cancellationToken)
	{
		if (!await IsChatExist(chatId, cancellationToken))
		{
			return None;
		}

		return await _dataConnection.GetTable<ChatMessageDb>()
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
		var chatId = new ChatId(Guid.CreateVersion7());
		participants = [..participants, _currentUserAccessor.CurrentUserId];
		var invalidUserId = (await _userProfileService.CheckUsersExistence(participants, cancellationToken))
			.FirstOrDefault(x => !x.Exists).UserId;
		if (invalidUserId != default)
		{
			return CreateChatError.ParticipantUserNotFound(invalidUserId);
		}

		await _chatParticipantsTable
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

		await using var transaction = await _dataConnection.BeginTransactionAsync(cancellationToken);
		var messageId = await _dataConnection.InsertWithInt64IdentityAsync(
			new ChatMessageDb
			{
				Timestamp = DateTimeOffset.UtcNow,
				ChatId = chatId,
				SenderId = _currentUserAccessor.CurrentUserId,
				Text = text,
			}, token: cancellationToken);

		await _integrationEventService.PublishEvent(
			new MessageSentEvent
			{
				ChatId = chatId,
				SenderId = _currentUserAccessor.CurrentUserId,
				Text = text,
			},
			cancellationToken);

		await transaction.CommitAsync(cancellationToken);

		return Right(new MessageId(messageId));
	}

	private Task<bool> IsChatExist(ChatId chatId, CancellationToken cancellationToken) =>
		_chatParticipantsTable
			.AnyAsync(x => x.ChatId == chatId && x.UserId == _currentUserAccessor.CurrentUserId, cancellationToken);
}