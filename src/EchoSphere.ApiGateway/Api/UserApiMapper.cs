using System.Security.Claims;
using EchoSphere.ApiGateway.Contracts;
using EchoSphere.ApiGateway.Extensions;
using EchoSphere.Users.Abstractions;
using EchoSphere.Users.Abstractions.Models;

namespace EchoSphere.ApiGateway.Api;

public static class UserApiMapper
{
	public static IEndpointRouteBuilder MapUserApi(this IEndpointRouteBuilder routeBuilder)
	{
		var userApi = routeBuilder.MapGroup("/users").RequireAuthorization();

		userApi.MapGet(
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

		userApi.MapGet(
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

		userApi.MapGet(
			"/{userId}/friends",
			async (IFriendService friendService, ClaimsPrincipal currentUser, string userId,
				CancellationToken cancellationToken) =>
			{
				var friends = await friendService.GetFriends(ParseUserId(userId, currentUser), cancellationToken);
				return friends.Select(x => x.Value);
			});

		userApi.MapPost(
			"/{toUserId:guid}/sendFriendInvite",
			(IFriendService friendService, ClaimsPrincipal currentUser, Guid toUserId,
				CancellationToken cancellationToken) =>
				friendService.SendFriendInvite(currentUser.GetUserId(), new UserId(toUserId), cancellationToken));

		userApi.MapGet(
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

		userApi.MapPost(
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

		userApi.MapPost(
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

		return routeBuilder;
	}

	private static UserId ParseUserId(string userId, ClaimsPrincipal currentUser) =>
		userId.Equals("me", StringComparison.OrdinalIgnoreCase) ? currentUser.GetUserId() : new UserId(Guid.Parse(userId));
}