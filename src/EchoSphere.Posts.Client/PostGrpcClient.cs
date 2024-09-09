using EchoSphere.GrpcModels;
using EchoSphere.Posts.Abstractions;
using EchoSphere.Posts.Abstractions.Models;
using EchoSphere.Posts.Grpc;
using EchoSphere.Users.Abstractions.Models;

namespace EchoSphere.Posts.Client;

public sealed class PostGrpcClient : IPostService
{
	private readonly PostService.PostServiceClient _serviceGrpcClient;

	public PostGrpcClient(PostService.PostServiceClient serviceGrpcClient)
	{
		_serviceGrpcClient = serviceGrpcClient;
	}

	public async ValueTask<PostId> PublishPost(UserId userId, string title, string body, CancellationToken cancellationToken)
	{
		var postId = await _serviceGrpcClient.PublishPostAsync(
			new PublishPostRequest
			{
				UserId = userId.Value.ToString(),
				Title = title,
				Body = body,
			},
			cancellationToken: cancellationToken);
		return new PostId(Guid.Parse(postId.Value));
	}

	public async ValueTask<IReadOnlyList<Post>> GetUserPosts(UserId userId, CancellationToken cancellationToken)
	{
		var posts = await _serviceGrpcClient.GetUserPostsAsync(
			new UserIdDto { Value = userId.Value.ToString() }, cancellationToken: cancellationToken);
		return posts.Posts
			.Select(x => new Post
			{
				Id = new PostId(Guid.Parse(x.Id)),
				Title = x.Title,
				Body = x.Body,
			})
			.ToArray();
	}
}