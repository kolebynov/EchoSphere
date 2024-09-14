using EchoSphere.GrpcModels;
using EchoSphere.SharedModels.Extensions;
using EchoSphere.Users.Abstractions;
using EchoSphere.Users.Abstractions.Models;
using EchoSphere.Users.Grpc;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace EchoSphere.Users.Api.GrpcServices;

internal sealed class FriendServiceGrpc : FriendService.FriendServiceBase
{
	private readonly IFriendService _friendService;

	public FriendServiceGrpc(IFriendService friendService)
	{
		_friendService = friendService;
	}

	public override Task<UserIdsDto> GetFriends(UserIdDto request, ServerCallContext context) =>
		_friendService.GetFriends(IdValueExtensions.Parse<UserId>(request.Value), context.CancellationToken)
			.Map(friendsOpt => friendsOpt
				.Map(friends => new UserIdsDto
				{
					Ids = { friends.Select(x => x.ToInnerString()) },
				})
				.IfNone(() => throw new RpcException(new Status(StatusCode.NotFound, "User not found."))));

	public override Task<Empty> SendFriendInvite(FromToUserIds request, ServerCallContext context)
	{
		return _friendService
			.SendFriendInvite(
				IdValueExtensions.Parse<UserId>(request.FromUserId), IdValueExtensions.Parse<UserId>(request.ToUserId),
				context.CancellationToken)
			.Map(either => either
				.Map(_ => new Empty())
				.IfLeft(_ => throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid request."))));
	}

	public override Task<GetFriendInvitesResponse> GetFriendInvites(UserIdDto request, ServerCallContext context) =>
		_friendService.GetFriendInvites(IdValueExtensions.Parse<UserId>(request.Value), context.CancellationToken)
			.Map(invitesOpt => invitesOpt
				.Map(invites => new GetFriendInvitesResponse
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
				.IfNone(() => throw new RpcException(new Status(StatusCode.NotFound, "User not found."))));

	public override Task<Empty> AcceptFriendInvite(FriendInvitationIdDto request, ServerCallContext context) =>
		_friendService.AcceptFriendInvite(IdValueExtensions.Parse<FriendInvitationId>(request.Value), context.CancellationToken)
			.Map(either => either
				.Map(_ => new Empty())
				.IfLeft(_ => throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid request."))));

	public override Task<Empty> RejectFriendInvite(FriendInvitationIdDto request, ServerCallContext context) =>
		_friendService.RejectFriendInvite(IdValueExtensions.Parse<FriendInvitationId>(request.Value), context.CancellationToken)
			.Map(either => either
				.Map(_ => new Empty())
				.IfLeft(_ => throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid request."))));
}