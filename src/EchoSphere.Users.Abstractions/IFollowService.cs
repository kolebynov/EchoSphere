using Dusharp;
using EchoSphere.Domain.Abstractions.Models;

namespace EchoSphere.Users.Abstractions;

[Union]
public partial struct FollowError
{
	[UnionCase]
	public static partial FollowError FollowerUserNotFound();

	[UnionCase]
	public static partial FollowError FollowUserNotFound();

	[UnionCase]
	public static partial FollowError AlreadyFollowed();
}

public interface IFollowService
{
	Task<Either<FollowError, Unit>> Follow(UserId followerUserId, UserId followUserId,
		CancellationToken cancellationToken);

	Task<Option<IReadOnlyList<UserId>>> GetFollowers(UserId userId, CancellationToken cancellationToken);
}