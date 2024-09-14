using System.Data;
using EchoSphere.Users.Abstractions;
using EchoSphere.Users.Abstractions.Extensions;
using EchoSphere.Users.Abstractions.Models;
using EchoSphere.Users.Api.Data.Models;
using LinqToDB;
using LinqToDB.Data;

namespace EchoSphere.Users.Api.Services;

internal sealed class FriendService : IFriendService
{
	private readonly DataConnection _dataConnection;
	private readonly IFollowService _followService;
	private readonly IUserProfileService _userProfileService;

	public FriendService(DataConnection dataConnection, IFollowService followService,
		IUserProfileService userProfileService)
	{
		_dataConnection = dataConnection;
		_followService = followService;
		_userProfileService = userProfileService;
	}

	public Task<Option<IReadOnlyList<UserId>>> GetFriends(UserId userId, CancellationToken cancellationToken) =>
		_userProfileService.CheckUsersExistence([userId], cancellationToken)
			.MapAsync(async usersExistence =>
			{
				if (!usersExistence[0].Exists)
				{
					return None;
				}

				var friendLinks = _dataConnection.GetTable<FriendLinkDb>();
				IReadOnlyList<UserId> result = await friendLinks
					.Where(x => x.User1Id == userId)
					.Select(x => x.User2Id)
					.Concat(friendLinks.Where(x => x.User2Id == userId).Select(x => x.User1Id))
					.ToArrayAsync(cancellationToken);
				return Some(result);
			});

	public Task<Either<SendFriendInviteError, Unit>> SendFriendInvite(UserId fromUserId, UserId toUserId,
		CancellationToken cancellationToken)
	{
		if (fromUserId == toUserId)
		{
			return Task.FromResult(Either<SendFriendInviteError, Unit>.Left(SendFriendInviteError.InvalidToUserId()));
		}

		return _userProfileService.CheckUsersExistence([fromUserId, toUserId], cancellationToken)
			.MapAsync(async usersExistence =>
			{
				if (!usersExistence[0].Exists)
				{
					return Either<SendFriendInviteError, Unit>.Left(SendFriendInviteError.InvalidFromUserId());
				}

				if (!usersExistence[1].Exists)
				{
					return SendFriendInviteError.InvalidToUserId();
				}

				var friendInvites = _dataConnection.GetTable<FriendInvitationDb>();
				if (await friendInvites.AnyAsync(x => x.FromUserId == fromUserId && x.ToUserId == toUserId, cancellationToken))
				{
					return SendFriendInviteError.AlreadySent();
				}

				if (await IsFriends(fromUserId, toUserId, cancellationToken))
				{
					return SendFriendInviteError.AlreadyFriend();
				}

				return Unit.Default;
			})
			.DoAsync(_ => _dataConnection.InsertAsync(
				new FriendInvitationDb { Id = new FriendInvitationId(Guid.NewGuid()), FromUserId = fromUserId, ToUserId = toUserId },
				token: cancellationToken));
	}

	public Task<Option<IReadOnlyList<FriendInvitation>>> GetFriendInvites(
		UserId userId, CancellationToken cancellationToken) =>
		_userProfileService.CheckUsersExistence([userId], cancellationToken)
			.MapAsync(async usersExistence =>
			{
				if (!usersExistence[0].Exists)
				{
					return None;
				}

				IReadOnlyList<FriendInvitation> result = await _dataConnection.GetTable<FriendInvitationDb>()
					.Where(x => x.ToUserId == userId)
					.Select(x => new FriendInvitation
					{
						Id = x.Id,
						FromUserId = x.FromUserId,
					})
					.ToArrayAsync(cancellationToken);
				return Some(result);
			});

	public Task<Either<FriendInviteError, Unit>> AcceptFriendInvite(
		FriendInvitationId invitationId, CancellationToken cancellationToken) =>
		GetInvitation(invitationId, cancellationToken)
			.Map(x => x.ToEither(FriendInviteError.InvalidInvitationId()))
			.DoAsync(async invitation =>
			{
				var (user1Id, user2Id) = SortUserIds(invitation.FromUserId, invitation.ToUserId);

				await using var transaction =
					await _dataConnection.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
				await _dataConnection.GetTable<FriendInvitationDb>()
					.DeleteAsync(x => x.Id == invitationId, cancellationToken);
				await _dataConnection.InsertAsync(
					new FriendLinkDb { User1Id = user1Id, User2Id = user2Id },
					token: cancellationToken);
				await _followService.Follow(user1Id, user2Id, cancellationToken);
				await _followService.Follow(user2Id, user1Id, cancellationToken);
				await transaction.CommitAsync(cancellationToken);
			})
			.Map(either => either.Map(_ => Unit.Default));

	public Task<Either<FriendInviteError, Unit>> RejectFriendInvite(
		FriendInvitationId invitationId, CancellationToken cancellationToken) =>
		_dataConnection.GetTable<FriendInvitationDb>()
			.DeleteAsync(x => x.Id == invitationId, cancellationToken)
			.Map(deleted =>
				deleted > 0
					? Either<FriendInviteError, Unit>.Right(Unit.Default)
					: FriendInviteError.InvalidInvitationId());

	private Task<Option<FriendInvitationDb>> GetInvitation(
		FriendInvitationId invitationId, CancellationToken cancellationToken) =>
		_dataConnection.GetTable<FriendInvitationDb>()
			.FirstOrDefaultAsync(x => x.Id == invitationId, cancellationToken)
			.Map(Optional);

	private Task<bool> IsFriends(UserId user1Id, UserId user2Id, CancellationToken cancellationToken)
	{
		(user1Id, user2Id) = SortUserIds(user1Id, user2Id);
		return _dataConnection.GetTable<FriendLinkDb>()
			.AnyAsync(x => x.User1Id == user1Id && x.User2Id == user2Id, cancellationToken);
	}

	private static (UserId User1Id, UserId User2Id) SortUserIds(UserId user1Id, UserId user2Id) =>
		user1Id.Value <= user2Id.Value ? (user1Id, user2Id) : (user2Id, user1Id);
}