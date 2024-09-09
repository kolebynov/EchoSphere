using EchoSphere.Users.Abstractions.Models;

namespace EchoSphere.Users.Abstractions;

public interface IUserProfileService
{
	ValueTask<IReadOnlyList<UserProfile>> GetUserProfiles(CancellationToken cancellationToken);

	ValueTask<UserProfile> GetUserProfile(UserId userId, CancellationToken cancellationToken);
}