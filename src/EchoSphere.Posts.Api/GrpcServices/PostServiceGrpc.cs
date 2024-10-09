using EchoSphere.Domain.Abstractions.Extensions;
using EchoSphere.Domain.Abstractions.Models;
using EchoSphere.GrpcShared.Contracts;
using EchoSphere.GrpcShared.Errors;
using EchoSphere.GrpcShared.Extensions;
using EchoSphere.Posts.Abstractions;
using EchoSphere.Posts.Grpc;
using EchoSphere.Users.Abstractions.Extensions;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;

namespace EchoSphere.Posts.Api.GrpcServices;

[Authorize]
internal sealed class PostServiceGrpc : PostService.PostServiceBase
{
	private readonly IPostService _postService;

	public PostServiceGrpc(IPostService postService)
	{
		_postService = postService;
	}

	public override Task<PostIdDto> PublishPost(PublishPostRequest request, ServerCallContext context) =>
		_postService
			.PublishPost(request.Title, request.Body, context.CancellationToken)
			.MapAsync(postId => postId.ToDto())
			.IfLeft(err => throw err
				.Match(() => new PublishPostErrorDto { InvalidUser = GrpcExtensions.EmptyInstance })
				.ToStatusRpcException());

	public override Task<PostsDto> GetUserPosts(UserIdDto request, ServerCallContext context) =>
		_postService.GetUserPosts(IdValueExtensions.Parse<UserId>(request.Value), context.CancellationToken)
			.MapAsync(posts => new PostsDto
			{
				Posts =
				{
					posts.Select(x => new PostDto { Id = x.Id.ToInnerString(), Title = x.Title, Body = x.Body }),
				},
			})
			.IfNone(() => throw NotFoundError.Instance.ToStatusRpcException());
}