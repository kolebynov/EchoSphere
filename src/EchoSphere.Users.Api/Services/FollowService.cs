using EchoSphere.Users.Abstractions;
using EchoSphere.Users.Abstractions.Models;
using EchoSphere.Users.Api.Data.Models;
using LinqToDB;
using LinqToDB.Data;

namespace EchoSphere.Users.Api.Services;

internal sealed class FollowService : IFollowService
{
	private readonly DataConnection _dataConnection;

	public FollowService(DataConnection dataConnection)
	{
		_dataConnection = dataConnection;
	}

	public async ValueTask Follow(UserId followerUserId, UserId followUserId, CancellationToken cancellationToken)
	{
		var followers = _dataConnection.GetTable<FollowerDb>();
		if (await followers.AnyAsync(
			    x => x.UserId == followUserId && x.FollowerUserId == followerUserId, cancellationToken))
		{
			return;
		}

		await _dataConnection.InsertAsync(
			new FollowerDb { UserId = followUserId, FollowerUserId = followerUserId }, token: cancellationToken);
	}

	public async ValueTask<IReadOnlyList<UserId>> GetFollowers(UserId userId, CancellationToken cancellationToken)
	{
		return await _dataConnection.GetTable<FollowerDb>()
			.Where(x => x.UserId == userId)
			.Select(x => x.FollowerUserId)
			.ToArrayAsync(cancellationToken);
	}
}