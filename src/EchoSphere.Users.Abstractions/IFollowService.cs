using Dusharp;
using EchoSphere.Users.Abstractions.Models;

namespace EchoSphere.Users.Abstractions;

[Union]
public partial class FollowError
{
	[UnionCase]
	public static partial FollowError InvalidFollowerId();

	[UnionCase]
	public static partial FollowError InvalidFollowId();

	[UnionCase]
	public static partial FollowError AlreadyFollowed();
}

public interface IFollowService
{
	Task<Either<FollowError, Unit>> Follow(UserId followerUserId, UserId followUserId,
		CancellationToken cancellationToken);

	Task<Option<IReadOnlyList<UserId>>> GetFollowers(UserId userId, CancellationToken cancellationToken);
}