using System.Security.Claims;
using EchoSphere.ApiGateway.Contracts;
using EchoSphere.ApiGateway.Extensions;
using EchoSphere.Domain.Abstractions.Extensions;
using EchoSphere.Domain.Abstractions.Models;
using EchoSphere.Users.Abstractions;
using EchoSphere.Users.Abstractions.Extensions;
using EchoSphere.Users.Abstractions.Models;
using Microsoft.AspNetCore.Http.HttpResults;

namespace EchoSphere.ApiGateway.Api;

public static class UsersApiMapper
{
	public static IEndpointRouteBuilder MapUsersApi(this IEndpointRouteBuilder routeBuilder)
	{
		var usersApi = routeBuilder.MapGroup("/users").RequireAuthorization();
		var friendInvitesApi = routeBuilder.MapGroup("/friendInvites").RequireAuthorization();

		usersApi.MapGet("/", GetUserProfiles);
		usersApi.MapGet("/{userId}", GetUserProfile);
		usersApi.MapGet("/{userId}/basicProfile", GetBasicUserProfile);
		usersApi.MapGet("/{userId}/friends", GetUserFriends);
		usersApi.MapPost("/{toUserId:guid}/sendFriendInvite", SendFriendInvite);
		usersApi.MapPost("/{followUserId:guid}/follow", Follow);
		usersApi.MapGet("/{userId}/followers", GetFollowers);

		friendInvitesApi.MapGet("/", GetFriendInvites);
		friendInvitesApi.MapPost("/{invitationId:guid}/accept", AcceptInvitation);
		friendInvitesApi.MapPost("/{invitationId:guid}/reject", RejectInvitation);

		return routeBuilder;
	}

	// TODO: Return Option<UserId>
	public static UserId ParseUserId(string userId, ClaimsPrincipal currentUser) =>
		userId.Equals("me", StringComparison.OrdinalIgnoreCase)
			? currentUser.GetUserId()
			: IdValueExtensions.Parse<UserId>(userId);

	private static async Task<IEnumerable<UserProfileDtoV1>> GetUserProfiles(
		IUserProfileService userProfileService, CancellationToken cancellationToken)
	{
		var profiles = await userProfileService.GetUserProfiles(cancellationToken);
		return profiles
			.Select(x => new UserProfileDtoV1
			{
				Id = x.Id.Value,
				FirstName = x.FirstName,
				SecondName = x.SecondName,
			});
	}

	private static Task<Results<Ok<UserProfileDtoV1>, ProblemHttpResult>> GetUserProfile(
		IUserProfileService userProfileService, ClaimsPrincipal currentUser, string userId,
		CancellationToken cancellationToken) =>
		userProfileService.GetUserProfile(ParseUserId(userId, currentUser), cancellationToken)
			.MapAsync(profile => TypedResults.Ok(new UserProfileDtoV1
			{
				Id = profile.Id.Value,
				FirstName = profile.FirstName,
				SecondName = profile.SecondName,
			}).ToResults<Ok<UserProfileDtoV1>, ProblemHttpResult>())
			.IfNone(() => TypedResults.Problem(statusCode: 404));

	private static Task<Results<Ok<BasicUserProfileDtoV1>, ProblemHttpResult>> GetBasicUserProfile(
		IUserProfileService userProfileService, ClaimsPrincipal currentUser, string userId,
		CancellationToken cancellationToken) =>
		userProfileService.GetBasicUserProfile(ParseUserId(userId, currentUser), cancellationToken)
			.MapAsync(profile => TypedResults.Ok(new BasicUserProfileDtoV1
			{
				Id = profile.Id.Value,
				FirstName = profile.FirstName,
				SecondName = profile.SecondName,
			}).ToResults<Ok<BasicUserProfileDtoV1>, ProblemHttpResult>())
			.IfNone(() => TypedResults.Problem(statusCode: 404));

	private static Task<Results<Ok<IEnumerable<Guid>>, ProblemHttpResult>> GetUserFriends(
		IFriendService friendService, ClaimsPrincipal currentUser, string userId, CancellationToken cancellationToken) =>
		friendService.GetFriends(ParseUserId(userId, currentUser), cancellationToken)
			.MapAsync(friends => TypedResults.Ok(friends.Select(x => x.Value))
				.ToResults<Ok<IEnumerable<Guid>>, ProblemHttpResult>())
			.IfNone(() => TypedResults.Problem(statusCode: 404));

	private static Task<Results<Ok, ProblemHttpResult>> SendFriendInvite(
		IFriendService friendService, Guid toUserId, CancellationToken cancellationToken) =>
		friendService.SendFriendInvite(new UserId(toUserId), cancellationToken)
			.MapAsync(_ => TypedResults.Ok().ToResults<Ok, ProblemHttpResult>())
			.IfLeft(err => err.Match(
				() => TypedResults.Problem(statusCode: 400, type: "Invalid_from_user"),
				() => TypedResults.Problem(statusCode: 400, type: "Invalid_to_user"),
				() => TypedResults.Problem(statusCode: 400, type: "already_sent"),
				() => TypedResults.Problem(statusCode: 400, type: "already_friend")));

	private static Task<Results<Ok<IEnumerable<FriendInvitationDtoV1>>, ProblemHttpResult>> GetFriendInvites(
		IFriendService friendService, CancellationToken cancellationToken) =>
		friendService.GetCurrentUserFriendInvites(cancellationToken)
			.MapAsync(invites => TypedResults.Ok(invites.Select(x => new FriendInvitationDtoV1
			{
				Id = x.Id.Value,
				FromUserId = x.FromUserId.Value,
			})).ToResults<Ok<IEnumerable<FriendInvitationDtoV1>>, ProblemHttpResult>())
			.IfNone(() => TypedResults.Problem(statusCode: 404));

	private static Task<Results<Ok, ProblemHttpResult>> AcceptInvitation(
		IFriendService friendService, Guid invitationId, CancellationToken cancellationToken) =>
		friendService.AcceptFriendInvite(new FriendInvitationId(invitationId), cancellationToken)
			.MapAsync(_ => TypedResults.Ok().ToResults<Ok, ProblemHttpResult>())
			.IfLeft(err => err.Match(() => TypedResults.Problem(statusCode: 404, type: "invitation_not_found")));

	private static Task<Results<Ok, ProblemHttpResult>> RejectInvitation(
		IFriendService friendService, Guid invitationId, CancellationToken cancellationToken) =>
		friendService.RejectFriendInvite(new FriendInvitationId(invitationId), cancellationToken)
			.MapAsync(_ => TypedResults.Ok().ToResults<Ok, ProblemHttpResult>())
			.IfLeft(err => err.Match(() => TypedResults.Problem(statusCode: 404, type: "invitation_not_found")));

	private static Task<Results<Ok, ProblemHttpResult>> Follow(
		IFollowService followService, ClaimsPrincipal currentUser, Guid followUserId, CancellationToken cancellationToken) =>
		followService.Follow(currentUser.GetUserId(), new UserId(followUserId), cancellationToken)
			.MapAsync(_ => TypedResults.Ok().ToResults<Ok, ProblemHttpResult>())
			.IfLeft(err => err.Match(
				() => TypedResults.Problem(statusCode: 404, type: "user_not_found"),
				() => TypedResults.Problem(statusCode: 404, type: "follow_user_not_found"),
				() => TypedResults.Problem(statusCode: 400, type: "already_followed")));

	private static Task<Results<Ok<IEnumerable<Guid>>, ProblemHttpResult>> GetFollowers(
		IFollowService followService, ClaimsPrincipal currentUser, string userId,
		CancellationToken cancellationToken)
	{
		var parsedUserId = ParseUserId(userId, currentUser);
		return followService.GetFollowers(parsedUserId, cancellationToken)
			.MapAsync(followers => TypedResults.Ok(followers.Select(x => x.Value))
				.ToResults<Ok<IEnumerable<Guid>>, ProblemHttpResult>())
			.IfNone(() => TypedResults.Problem(statusCode: 404));
	}
}