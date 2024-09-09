using Refit;

namespace EchoSphere.ApiGateway.Client;

public interface IFollowClient
{
	[Post("/users/{followUserId}/follow")]
	Task Follow(Guid followUserId, CancellationToken cancellationToken);

	[Get("/users/{userId}/followers")]
	Task<IReadOnlyList<Guid>> GetFollowers(string userId, CancellationToken cancellationToken);
}