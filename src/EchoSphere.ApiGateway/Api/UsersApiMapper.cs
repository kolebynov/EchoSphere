using System.Security.Claims;
using EchoSphere.ApiGateway.Contracts;
using EchoSphere.ApiGateway.Extensions;
using EchoSphere.SharedModels.Extensions;
using EchoSphere.Users.Abstractions;
using EchoSphere.Users.Abstractions.Models;
using LanguageExt;
using LanguageExt.UnsafeValueAccess;

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
				var profile = (await userProfileService.GetUserProfile(ParseUserId(userId, currentUser), cancellationToken)).ValueUnsafe();
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
				return friends.ValueUnsafe().Select(x => x.Value);
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

				var invites = (await friendService.GetFriendInvites(currentUserId, cancellationToken)).ValueUnsafe();
				return invites.Select(x => new FriendInvitationDtoV1
				{
					Id = x.Id.Value,
					FromUserId = x.FromUserId.Value,
				});
			});

		usersApi.MapPost(
			"/{userId}/friendInvites/{invitationId:guid}/accept",
			(IFriendService friendService, ClaimsPrincipal currentUser, string userId, Guid invitationId,
				CancellationToken cancellationToken) =>
			{
				var currentUserId = currentUser.GetUserId();
				var parsedUserId = ParseUserId(userId, currentUser);
				if (currentUserId != parsedUserId)
				{
					return Task.CompletedTask;
				}

				return friendService.AcceptFriendInvite(new FriendInvitationId(invitationId), cancellationToken)
					.Map(x => x.ValueUnsafe());
			});

		usersApi.MapPost(
			"/{userId}/friendInvites/{invitationId:guid}/reject",
			(IFriendService friendService, ClaimsPrincipal currentUser, string userId, Guid invitationId,
				CancellationToken cancellationToken) =>
			{
				var currentUserId = currentUser.GetUserId();
				var parsedUserId = ParseUserId(userId, currentUser);
				if (currentUserId != parsedUserId)
				{
					return Task.CompletedTask;
				}

				return friendService.RejectFriendInvite(new FriendInvitationId(invitationId), cancellationToken)
					.Map(x => x.ValueUnsafe());
			});

		usersApi.MapPost(
			"/{followUserId:guid}/follow",
			(IFollowService followService, ClaimsPrincipal currentUser, Guid followUserId,
				CancellationToken cancellationToken) =>
			{
				var currentUserId = currentUser.GetUserId();
				if (currentUserId.Value == followUserId)
				{
					return Task.CompletedTask;
				}

				return followService.Follow(currentUserId, new UserId(followUserId), cancellationToken)
					.Map(x => x.ValueUnsafe());
			});

		usersApi.MapGet(
			"/{userId}/followers",
			async (IFollowService followService, ClaimsPrincipal currentUser, string userId,
				CancellationToken cancellationToken) =>
			{
				var parsedUserId = ParseUserId(userId, currentUser);
				var followers = (await followService.GetFollowers(parsedUserId, cancellationToken)).ValueUnsafe();
				return followers.Select(x => x.Value);
			});

		return routeBuilder;
	}

	// TODO: Return Option<UserId>
	public static UserId ParseUserId(string userId, ClaimsPrincipal currentUser) =>
		userId.Equals("me", StringComparison.OrdinalIgnoreCase)
			? currentUser.GetUserId()
			: IdValueExtensions.Parse<UserId>(userId);
}