using EchoSphere.GrpcModels;
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

	public override async Task<UserIdsDto> GetFriends(UserIdDto request, ServerCallContext context)
	{
		var friends = await _friendService.GetFriends(new UserId(Guid.Parse(request.Value)), context.CancellationToken);
		return new UserIdsDto
		{
			Ids = { friends.Select(x => x.Value.ToString()) },
		};
	}

	public override async Task<Empty> SendFriendInvite(FromToUserIds request, ServerCallContext context)
	{
		await _friendService.SendFriendInvite(
			new UserId(Guid.Parse(request.FromUserId)), new UserId(Guid.Parse(request.ToUserId)),
			context.CancellationToken);
		return new Empty();
	}

	public override async Task<UserIdsDto> GetFriendInvites(UserIdDto request, ServerCallContext context)
	{
		var invites =
			await _friendService.GetFriendInvites(new UserId(Guid.Parse(request.Value)), context.CancellationToken);
		return new UserIdsDto
		{
			Ids = { invites.Select(x => x.Value.ToString()) },
		};
	}

	public override async Task<Empty> AcceptFriendInvite(FromToUserIds request, ServerCallContext context)
	{
		await _friendService.AcceptFriendInvite(
			new UserId(Guid.Parse(request.FromUserId)), new UserId(Guid.Parse(request.ToUserId)),
			context.CancellationToken);
		return new Empty();
	}

	public override async Task<Empty> RejectFriendInvite(FromToUserIds request, ServerCallContext context)
	{
		await _friendService.RejectFriendInvite(
			new UserId(Guid.Parse(request.FromUserId)), new UserId(Guid.Parse(request.ToUserId)),
			context.CancellationToken);
		return new Empty();
	}
}