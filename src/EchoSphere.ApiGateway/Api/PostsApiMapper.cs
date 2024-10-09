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
					.PublishPost(request.Title, request.Body, cancellationToken)
					.MapAsync(postId => Results.Ok(postId.Value))
					.IfLeft(err => err.Match(() => Results.Problem(statusCode: 400)));
			});

		postsApi.MapGet(
			"/",
			(IPostService postService, ClaimsPrincipal currentUser, CancellationToken cancellationToken) =>
				GetUserPosts(postService, currentUser.GetUserId(), cancellationToken));

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
					Title = x.Title,
					Body = x.Body,
				})).ToResults<Ok<IEnumerable<PostDtoV1>>, ProblemHttpResult>())
			.IfNone(() => TypedResults.Problem(statusCode: 404));
}