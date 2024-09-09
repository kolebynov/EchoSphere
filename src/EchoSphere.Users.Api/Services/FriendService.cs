using System.Data;
using System.Linq.Expressions;
using EchoSphere.Users.Abstractions;
using EchoSphere.Users.Abstractions.Models;
using EchoSphere.Users.Api.Data.Models;
using LinqToDB;
using LinqToDB.Data;

namespace EchoSphere.Users.Api.Services;

internal sealed class FriendService : IFriendService
{
	private readonly DataConnection _dataConnection;
	private readonly IFollowService _followService;

	public FriendService(DataConnection dataConnection, IFollowService followService)
	{
		_dataConnection = dataConnection;
		_followService = followService;
	}

	public async ValueTask<IReadOnlyList<UserId>> GetFriends(UserId userId, CancellationToken cancellationToken)
	{
		var friendLinks = _dataConnection.GetTable<FriendLinkDb>();
		return await friendLinks
			.Where(x => x.User1Id == userId)
			.Select(x => x.User2Id)
			.Concat(friendLinks.Where(x => x.User2Id == userId).Select(x => x.User1Id))
			.ToArrayAsync(cancellationToken);
	}

	public async ValueTask SendFriendInvite(UserId fromUserId, UserId toUserId, CancellationToken cancellationToken)
	{
		var friendInvites = _dataConnection.GetTable<FriendInviteDb>();
		if (fromUserId == toUserId ||
		    await friendInvites.AnyAsync(x => x.FromUserId == fromUserId && x.ToUserId == toUserId, cancellationToken) ||
		    await IsFriends(fromUserId, toUserId, cancellationToken))
		{
			return;
		}

		await _dataConnection.InsertAsync(
			new FriendInviteDb { FromUserId = fromUserId, ToUserId = toUserId },
			token: cancellationToken);
	}

	public async ValueTask<IReadOnlyList<UserId>> GetFriendInvites(UserId userId, CancellationToken cancellationToken)
	{
		return await _dataConnection.GetTable<FriendInviteDb>()
			.Where(x => x.ToUserId == userId)
			.Select(x => x.FromUserId)
			.ToArrayAsync(cancellationToken);
	}

	public async ValueTask AcceptFriendInvite(UserId fromUserId, UserId toUserId, CancellationToken cancellationToken)
	{
		var friendInvites = _dataConnection.GetTable<FriendInviteDb>();
		Expression<Func<FriendInviteDb, bool>> friendInvitePredicate =
			x => x.FromUserId == fromUserId && x.ToUserId == toUserId;

		if (!await friendInvites.AnyAsync(friendInvitePredicate, cancellationToken))
		{
			return;
		}

		var (user1Id, user2Id) = SortUserIds(fromUserId, toUserId);

		await using var transaction = await _dataConnection.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
		await friendInvites.DeleteAsync(friendInvitePredicate, cancellationToken);
		await _dataConnection.InsertAsync(new FriendLinkDb { User1Id = user1Id, User2Id = user2Id }, token: cancellationToken);
		await _followService.Follow(user1Id, user2Id, cancellationToken);
		await _followService.Follow(user2Id, user1Id, cancellationToken);
		await transaction.CommitAsync(cancellationToken);
	}

	public ValueTask RejectFriendInvite(UserId fromUserId, UserId toUserId, CancellationToken cancellationToken)
	{
		return new ValueTask(_dataConnection.GetTable<FriendInviteDb>()
			.DeleteAsync(x => x.FromUserId == fromUserId && x.ToUserId == toUserId, cancellationToken));
	}

	private ValueTask<bool> IsFriends(UserId user1Id, UserId user2Id, CancellationToken cancellationToken)
	{
		(user1Id, user2Id) = SortUserIds(user1Id, user2Id);
		return new ValueTask<bool>(
			_dataConnection.GetTable<FriendLinkDb>()
				.AnyAsync(x => x.User1Id == user1Id && x.User2Id == user2Id, cancellationToken));
	}

	private static (UserId User1Id, UserId User2Id) SortUserIds(UserId user1Id, UserId user2Id) =>
		user1Id.Value <= user2Id.Value ? (user1Id, user2Id) : (user2Id, user1Id);
}