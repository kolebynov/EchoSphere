using System.Security.Claims;
using EchoSphere.ApiGateway.Contracts;
using EchoSphere.ApiGateway.Extensions;
using EchoSphere.Posts.Abstractions;
using EchoSphere.Users.Abstractions.Models;
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
			async (IPostService postService, ClaimsPrincipal currentUser, [FromBody] PublishPostRequestV1 request,
				CancellationToken cancellationToken) =>
			{
				var postId = await postService.PublishPost(currentUser.GetUserId(), request.Title, request.Body,
					cancellationToken);
				return postId.Value;
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

	private static async ValueTask<IEnumerable<PostDtoV1>> GetUserPosts(IPostService postService, UserId userId,
		CancellationToken cancellationToken)
	{
		var posts = await postService.GetUserPosts(userId, cancellationToken);
		return posts
			.Select(x => new PostDtoV1
			{
				Id = x.Id.Value,
				Title = x.Title,
				Body = x.Body,
			});
	}
}