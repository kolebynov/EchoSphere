using System.Security.Claims;
using EchoSphere.ApiGateway.Contracts;
using EchoSphere.ApiGateway.Extensions;
using EchoSphere.Domain.Abstractions.Models;
using EchoSphere.Posts.Abstractions;
using EchoSphere.Users.Abstractions.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace EchoSphere.ApiGateway.Api;

public static class PostsApiMapper
{
	public static IEndpointRouteBuilder MapPostsApi(this IEndpointRouteBuilder routeBuilder)
	{
		var postsApi = routeBuilder.MapGroup("/posts").RequireAuthorization();
		var usersApi = routeBuilder.MapGroup("/users").RequireAuthorization();

		postsApi.MapPost(
			"/",
			(IPostService postService, [FromBody] PublishPostRequestV1 request,
				CancellationToken cancellationToken) =>
			{
				return postService
					.PublishPost(request.Body, cancellationToken)
					.MapAsync(postId => Results.Ok(postId.Value))
					.IfLeft(err => err.Match(() => Results.Problem(statusCode: 400)));
			});

		postsApi.MapGet(
			"/",
			(IPostService postService, ClaimsPrincipal currentUser, CancellationToken cancellationToken) =>
				GetUserPosts(postService, currentUser.GetUserId(), cancellationToken));

		postsApi.MapPost("/{postId:guid}/toggleLike", TogglePostLike);
		postsApi.MapGet("/{postId:guid}/comments", GetPostComments);
		postsApi.MapPost("/{postId:guid}/comments", AddComment);

		usersApi.MapGet(
			"/{userId}/posts",
			(IPostService postService, ClaimsPrincipal currentUser, string userId, CancellationToken cancellationToken) =>
			{
				var parsedUserId = UsersApiMapper.ParseUserId(userId, currentUser);
				return GetUserPosts(postService, parsedUserId, cancellationToken);
			});

		return routeBuilder;
	}

	private static Task<Results<Ok<IEnumerable<PostDtoV1>>, ProblemHttpResult>> GetUserPosts(
		IPostService postService, UserId userId, CancellationToken cancellationToken) =>
		postService.GetUserPosts(userId, cancellationToken)
			.MapAsync(posts => TypedResults.Ok(posts
				.Select(x => new PostDtoV1
				{
					Id = x.Id.Value,
					PostedOn = x.PostedOn,
					AuthorId = x.AuthorId.Value,
					Body = x.Body,
					LikedByCurrentUser = x.LikedByCurrentUser,
					LikesCount = x.LikesCount,
					CommentsCount = x.CommentsCount,
				})).ToResults<Ok<IEnumerable<PostDtoV1>>, ProblemHttpResult>())
			.IfNone(() => TypedResults.Problem(statusCode: 404));

	private static Task<Results<Ok, ProblemHttpResult>> TogglePostLike(IPostService postService, Guid postId,
		CancellationToken cancellationToken) =>
		postService.TogglePostLike(new PostId(postId), cancellationToken)
			.MapAsync(_ => TypedResults.Ok().ToResults<Ok, ProblemHttpResult>())
			.IfLeft(err => err.Match(
				() => TypedResults.Problem(statusCode: 404)));

	private static Task<Results<Ok<IEnumerable<PostCommentDtoV1>>, ProblemHttpResult>> GetPostComments(
		IPostService postService, Guid postId, CancellationToken cancellationToken) =>
		postService.GetPostComments(new PostId(postId), cancellationToken)
			.MapAsync(posts => TypedResults.Ok(posts
				.Select(x => new PostCommentDtoV1
				{
					Id = x.Id.Value,
					Text = x.Text,
					UserId = x.UserId.Value,
				})).ToResults<Ok<IEnumerable<PostCommentDtoV1>>, ProblemHttpResult>())
			.IfNone(() => TypedResults.Problem(statusCode: 404));

	private static Task<Results<Ok<Guid>, ProblemHttpResult>> AddComment(IPostService postService, Guid postId,
		[FromBody] AddPostCommentRequestV1 request, CancellationToken cancellationToken) =>
		postService.AddComment(new PostId(postId), request.Text, cancellationToken)
			.MapAsync(x => TypedResults.Ok(x.Value).ToResults<Ok<Guid>, ProblemHttpResult>())
			.IfLeft(err => err.Match(
				() => TypedResults.Problem(statusCode: 404)));
}