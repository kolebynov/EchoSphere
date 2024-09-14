using EchoSphere.Users.Abstractions.Models;

namespace EchoSphere.Users.Abstractions;

public interface IUserProfileService
{
	Task<IReadOnlyList<UserProfile>> GetUserProfiles(CancellationToken cancellationToken);

	Task<Option<UserProfile>> GetUserProfile(UserId userId, CancellationToken cancellationToken);

	Task<IReadOnlyList<(UserId UserId, bool Exists)>> CheckUsersExistence(
		IReadOnlyList<UserId> userIds, CancellationToken cancellationToken);
}