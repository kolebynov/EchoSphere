using EchoSphere.ApiGateway.Contracts;
using Refit;

namespace EchoSphere.ApiGateway.Client;

public interface IPostClient
{
	[Post("/posts")]
	Task PublishPost([Body] PublishPostRequestV1 request, CancellationToken cancellationToken);

	[Get("/posts")]
	Task<IReadOnlyList<PostDtoV1>> GetCurrentUserPosts(CancellationToken cancellationToken);

	[Get("/users/{userId}/posts")]
	Task<IReadOnlyList<PostDtoV1>> GetUserPosts(string userId, CancellationToken cancellationToken);
}