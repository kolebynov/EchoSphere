using System.Security.Claims;
using EchoSphere.ApiGateway.Contracts;
using EchoSphere.ApiGateway.Extensions;
using EchoSphere.Users.Abstractions;
using EchoSphere.Users.Abstractions.Models;

namespace EchoSphere.ApiGateway.Api;

public static class UsersApiMapper
{
	public static IEndpointRouteBuilder MapUsersApi(this IEndpointRouteBuilder routeBuilder)
	{
		var usersApi = routeBuilder.MapGroup("/users").RequireAuthorization();

		usersApi.MapGet(
			"/",
			async (IUserProfileService userProfileService, CancellationToken cancellationToken) =>
			{
				var profiles = await userProfileService.GetUserProfiles(cancellationToken);
				return profiles
					.Select(x => new UserProfileDtoV1
					{
						Id = x.Id.Value,
						FirstName = x.FirstName,
						SecondName = x.SecondName,
					});
			});

		usersApi.MapGet(
			"/{userId}",
			async (IUserProfileService userProfileService, ClaimsPrincipal currentUser, string userId, CancellationToken cancellationToken) =>
			{
				var profile = await userProfileService.GetUserProfile(ParseUserId(userId, currentUser), cancellationToken);
				return new UserProfileDtoV1
				{
					Id = profile.Id.Value,
					FirstName = profile.FirstName,
					SecondName = profile.SecondName,
				};
			});

		usersApi.MapGet(
			"/{userId}/friends",
			async (IFriendService friendService, ClaimsPrincipal currentUser, string userId,
				CancellationToken cancellationToken) =>
			{
				var friends = await friendService.GetFriends(ParseUserId(userId, currentUser), cancellationToken);
				return friends.Select(x => x.Value);
			});

		usersApi.MapPost(
			"/{toUserId:guid}/sendFriendInvite",
			(IFriendService friendService, ClaimsPrincipal currentUser, Guid toUserId,
				CancellationToken cancellationToken) =>
				friendService.SendFriendInvite(currentUser.GetUserId(), new UserId(toUserId), cancellationToken));

		usersApi.MapGet(
			"/{userId}/friendInvites",
			async (IFriendService friendService, ClaimsPrincipal currentUser, string userId,
				CancellationToken cancellationToken) =>
			{
				var currentUserId = currentUser.GetUserId();
				var parsedUserId = ParseUserId(userId, currentUser);
				if (currentUserId != parsedUserId)
				{
					return [];
				}

				var invites = await friendService.GetFriendInvites(currentUserId, cancellationToken);
				return invites.Select(x => x.Value);
			});

		usersApi.MapPost(
			"/{userId}/friendInvites/{fromUserId:guid}/accept",
			(IFriendService friendService, ClaimsPrincipal currentUser, string userId, Guid fromUserId,
				CancellationToken cancellationToken) =>
			{
				var currentUserId = currentUser.GetUserId();
				var parsedUserId = ParseUserId(userId, currentUser);
				if (currentUserId != parsedUserId)
				{
					return ValueTask.CompletedTask;
				}

				return friendService.AcceptFriendInvite(new UserId(fromUserId), currentUserId, cancellationToken);
			});

		usersApi.MapPost(
			"/{userId}/friendInvites/{fromUserId:guid}/reject",
			(IFriendService friendService, ClaimsPrincipal currentUser, string userId, Guid fromUserId,
				CancellationToken cancellationToken) =>
			{
				var currentUserId = currentUser.GetUserId();
				var parsedUserId = ParseUserId(userId, currentUser);
				if (currentUserId != parsedUserId)
				{
					return ValueTask.CompletedTask;
				}

				return friendService.RejectFriendInvite(new UserId(fromUserId), currentUserId, cancellationToken);
			});

		usersApi.MapPost(
			"/{followUserId:guid}/follow",
			(IFollowService followService, ClaimsPrincipal currentUser, Guid followUserId,
				CancellationToken cancellationToken) =>
			{
				var currentUserId = currentUser.GetUserId();
				if (currentUserId.Value == followUserId)
				{
					return ValueTask.CompletedTask;
				}

				return followService.Follow(currentUserId, new UserId(followUserId), cancellationToken);
			});

		usersApi.MapGet(
			"/{userId}/followers",
			async (IFollowService followService, ClaimsPrincipal currentUser, string userId,
				CancellationToken cancellationToken) =>
			{
				var parsedUserId = ParseUserId(userId, currentUser);
				var followers = await followService.GetFollowers(parsedUserId, cancellationToken);
				return followers.Select(x => x.Value);
			});

		return routeBuilder;
	}

	public static UserId ParseUserId(string userId, ClaimsPrincipal currentUser) =>
		userId.Equals("me", StringComparison.OrdinalIgnoreCase) ? currentUser.GetUserId() : new UserId(Guid.Parse(userId));
}