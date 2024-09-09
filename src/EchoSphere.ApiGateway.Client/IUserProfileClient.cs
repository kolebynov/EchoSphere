using EchoSphere.ApiGateway.Contracts;
using Refit;

namespace EchoSphere.ApiGateway.Client;

public interface IUserProfileClient
{
	[Get("/users")]
	Task<IReadOnlyList<UserProfileDtoV1>> GetUserProfiles(CancellationToken cancellationToken);

	[Get("/users/{userId}")]
	Task<UserProfileDtoV1> GetUserProfile(Guid userId, CancellationToken cancellationToken);
}