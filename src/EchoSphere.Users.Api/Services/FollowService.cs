using EchoSphere.Domain.Abstractions;
using EchoSphere.Domain.Abstractions.Models;
using EchoSphere.Users.Abstractions;
using EchoSphere.Users.Abstractions.Extensions;
using EchoSphere.Users.Api.Data.Models;
using LinqToDB;
using LinqToDB.Data;

namespace EchoSphere.Users.Api.Services;

internal sealed class FollowService : IFollowService
{
	private readonly DataConnection _dataConnection;
	private readonly IUserProfileService _userProfileService;
	private readonly ICurrentUserAccessor _currentUserAccessor;

	public FollowService(DataConnection dataConnection, IUserProfileService userProfileService,
		ICurrentUserAccessor currentUserAccessor)
	{
		_dataConnection = dataConnection;
		_userProfileService = userProfileService;
		_currentUserAccessor = currentUserAccessor;
	}

	public Task<Either<FollowError, Unit>> Follow(UserId followUserId, CancellationToken cancellationToken) =>
		_userProfileService.CheckUsersExistence([_currentUserAccessor.CurrentUserId, followUserId], cancellationToken)
			.MapAsync(async usersExistence =>
			{
				if (!usersExistence[0].Exists)
				{
					return Either<FollowError, Unit>.Left(FollowError.CurrentUserNotFound());
				}

				if (!usersExistence[1].Exists)
				{
					return FollowError.FollowUserNotFound();
				}

				var followers = _dataConnection.GetTable<FollowerDb>();
				if (await followers.AnyAsync(
					    x => x.UserId == followUserId && x.FollowerUserId == _currentUserAccessor.CurrentUserId, cancellationToken))
				{
					return FollowError.AlreadyFollowed();
				}

				return Unit.Default;
			})
			.DoAsync(_ => _dataConnection.InsertAsync(
				new FollowerDb { UserId = followUserId, FollowerUserId = _currentUserAccessor.CurrentUserId }, token: cancellationToken));

	public Task<Option<IReadOnlyList<UserId>>> GetFollowers(UserId userId, CancellationToken cancellationToken) =>
		_userProfileService.CheckUsersExistence([userId], cancellationToken)
			.MapAsync(async usersExistence =>
			{
				if (!usersExistence[0].Exists)
				{
					return None;
				}

				IReadOnlyList<UserId> result = await _dataConnection.GetTable<FollowerDb>()
					.Where(x => x.UserId == userId)
					.Select(x => x.FollowerUserId)
					.ToArrayAsync(cancellationToken);
				return Some(result);
			});
}