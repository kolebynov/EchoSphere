using EchoSphere.Domain.Abstractions.Extensions;
using EchoSphere.Domain.Abstractions.Models;
using EchoSphere.GrpcShared.Contracts;
using EchoSphere.GrpcShared.Errors;
using EchoSphere.GrpcShared.Extensions;
using EchoSphere.Posts.Abstractions;
using EchoSphere.Posts.Grpc;
using EchoSphere.Users.Abstractions.Extensions;
using Google.Protobuf.WellKnownTypes;
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
				.Match(() => new PublishPostErrorDto { CurrentUserNotFound = GrpcExtensions.EmptyInstance })
				.ToStatusRpcException());

	public override Task<PostsDto> GetUserPosts(UserIdDto request, ServerCallContext context) =>
		_postService.GetUserPosts(request.ToModel(), context.CancellationToken)
			.MapAsync(posts => new PostsDto
			{
				Posts =
				{
					posts.Select(x => new PostDto
					{
						Id = x.Id.ToInnerString(), Title = x.Title, Body = x.Body,
						LikedByCurrentUser = x.LikedByCurrentUser,
						LikesCount = x.LikesCount,
					}),
				},
			})
			.IfNone(() => throw NotFoundError.Instance.ToStatusRpcException());

	public override Task<Empty> TogglePostLike(PostIdDto request, ServerCallContext context) =>
		_postService.TogglePostLike(request.ToModel(), context.CancellationToken)
			.MapAsync(_ => GrpcExtensions.EmptyInstance)
			.IfLeft(err => throw err
				.Match(() => new TogglePostLikeErrorDto { PostNotFound = GrpcExtensions.EmptyInstance })
				.ToStatusRpcException());

	public override Task<PostCommentsDto> GetPostComments(PostIdDto request, ServerCallContext context) =>
		_postService.GetPostComments(request.ToModel(), context.CancellationToken)
			.MapAsync(comments => new PostCommentsDto
			{
				Comments =
				{
					comments.Select(x => new PostCommentDto
					{
						Id = x.Id.ToInnerString(), UserId = x.UserId.ToInnerString(), Text = x.Text,
					}),
				},
			})
			.IfNone(() => throw NotFoundError.Instance.ToStatusRpcException());

	public override Task<PostCommentIdDto> AddComment(AddCommentRequest request, ServerCallContext context) =>
		_postService.AddComment(IdValueExtensions.Parse<PostId>(request.PostId), request.Text, context.CancellationToken)
			.MapAsync(x => new PostCommentIdDto { Value = x.ToInnerString() })
			.IfLeft(err => throw err
				.Match(() => new AddCommentErrorDto { PostNotFound = GrpcExtensions.EmptyInstance })
				.ToStatusRpcException());
}