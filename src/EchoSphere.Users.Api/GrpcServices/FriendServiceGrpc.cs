using EchoSphere.Domain.Abstractions.Extensions;
using EchoSphere.Domain.Abstractions.Models;
using EchoSphere.GrpcShared.Contracts;
using EchoSphere.GrpcShared.Errors;
using EchoSphere.GrpcShared.Extensions;
using EchoSphere.Users.Abstractions;
using EchoSphere.Users.Abstractions.Extensions;
using EchoSphere.Users.Abstractions.Models;
using EchoSphere.Users.Grpc;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;

namespace EchoSphere.Users.Api.GrpcServices;

[Authorize]
internal sealed class FriendServiceGrpc : FriendService.FriendServiceBase
{
	private readonly IFriendService _friendService;

	public FriendServiceGrpc(IFriendService friendService)
	{
		_friendService = friendService;
	}

	public override Task<UserIdsDto> GetFriends(UserIdDto request, ServerCallContext context) =>
		_friendService.GetFriends(request.ToModel(), context.CancellationToken)
			.MapAsync(friends => new UserIdsDto
			{
				Ids = { friends.Select(x => x.ToInnerString()) },
			})
			.IfNone(() => throw NotFoundError.Instance.ToStatusRpcException());

	public override Task<Empty> SendFriendInvite(UserIdDto request, ServerCallContext context) =>
		_friendService
			.SendFriendInvite(IdValueExtensions.Parse<UserId>(request.Value), context.CancellationToken)
			.MapAsync(_ => GrpcExtensions.EmptyInstance)
			.IfLeft(err => throw err
				.Match(
					() => new SendFriendInviteErrorDto { CurrentUserNotFound = GrpcExtensions.EmptyInstance },
					() => new SendFriendInviteErrorDto { ToUserNotFound = GrpcExtensions.EmptyInstance },
					() => new SendFriendInviteErrorDto { AlreadySent = GrpcExtensions.EmptyInstance },
					() => new SendFriendInviteErrorDto { AlreadyFriend = GrpcExtensions.EmptyInstance })
				.ToStatusRpcException());

	public override Task<GetFriendInvitesResponse> GetCurrentUserFriendInvites(Empty request, ServerCallContext context) =>
		_friendService.GetCurrentUserFriendInvites(context.CancellationToken)
			.MapAsync(invites => new GetFriendInvitesResponse
			{
				Invitations =
				{
					invites.Select(x => new FriendInvitationDto
					{
						Id = x.Id.ToInnerString(),
						FromUserId = x.FromUserId.ToInnerString(),
					}),
				},
			})
			.IfNone(() => throw NotFoundError.Instance.ToStatusRpcException());

	public override Task<Empty> AcceptFriendInvite(FriendInvitationIdDto request, ServerCallContext context) =>
		_friendService.AcceptFriendInvite(IdValueExtensions.Parse<FriendInvitationId>(request.Value), context.CancellationToken)
			.MapAsync(_ => GrpcExtensions.EmptyInstance)
			.IfLeft(err => throw err
				.Match(() => new FriendInviteErrorDto { InvitationNotFound = GrpcExtensions.EmptyInstance })
				.ToStatusRpcException());

	public override Task<Empty> RejectFriendInvite(FriendInvitationIdDto request, ServerCallContext context) =>
		_friendService.RejectFriendInvite(IdValueExtensions.Parse<FriendInvitationId>(request.Value), context.CancellationToken)
			.MapAsync(_ => GrpcExtensions.EmptyInstance)
			.IfLeft(err => throw err
				.Match(() => new FriendInviteErrorDto { InvitationNotFound = GrpcExtensions.EmptyInstance })
				.ToStatusRpcException());
}