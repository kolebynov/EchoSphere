using EchoSphere.GrpcModels;
using EchoSphere.Posts.Abstractions;
using EchoSphere.Posts.Grpc;
using EchoSphere.Users.Abstractions.Models;
using Grpc.Core;

namespace EchoSphere.Posts.Api.GrpcServices;

internal sealed class PostServiceGrpc : PostService.PostServiceBase
{
	private readonly IPostService _postService;

	public PostServiceGrpc(IPostService postService)
	{
		_postService = postService;
	}

	public override async Task<PostIdDto> PublishPost(PublishPostRequest request, ServerCallContext context)
	{
		var postId = await _postService.PublishPost(new UserId(Guid.Parse(request.UserId)), request.Title, request.Body,
			context.CancellationToken);
		return new PostIdDto { Value = postId.Value.ToString() };
	}

	public override async Task<PostsDto> GetUserPosts(UserIdDto request, ServerCallContext context)
	{
		var posts = await _postService.GetUserPosts(new UserId(Guid.Parse(request.Value)), context.CancellationToken);
		return new PostsDto
		{
			Posts =
			{
				posts.Select(x => new PostDto { Id = x.Id.Value.ToString(), Title = x.Title, Body = x.Body }),
			},
		};
	}
}