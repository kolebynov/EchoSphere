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

	[Post("/posts/{postId}/toggleLike")]
	Task TogglePostLike(Guid postId, CancellationToken cancellationToken);

	[Get("/posts/{postId}/comments")]
	Task<IReadOnlyList<PostCommentDtoV1>> GetPostComments(Guid postId, CancellationToken cancellationToken);

	[Post("/posts/{postId}/comments")]
	Task<Guid> AddComment(Guid postId, [Body] AddPostCommentRequestV1 request, CancellationToken cancellationToken);
}