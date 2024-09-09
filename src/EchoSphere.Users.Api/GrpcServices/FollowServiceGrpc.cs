using EchoSphere.GrpcModels;
using EchoSphere.Users.Abstractions;
using EchoSphere.Users.Abstractions.Models;
using EchoSphere.Users.Grpc;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace EchoSphere.Users.Api.GrpcServices;

internal sealed class FollowServiceGrpc : FollowService.FollowServiceBase
{
	private readonly IFollowService _followService;

	public FollowServiceGrpc(IFollowService followService)
	{
		_followService = followService;
	}

	public override async Task<Empty> Follow(FromToUserIds request, ServerCallContext context)
	{
		await _followService.Follow(
			new UserId(Guid.Parse(request.FromUserId)), new UserId(Guid.Parse(request.ToUserId)),
			context.CancellationToken);
		return new Empty();
	}

	public override async Task<UserIdsDto> GetFollowers(UserIdDto request, ServerCallContext context)
	{
		var followers = await _followService.GetFollowers(new UserId(Guid.Parse(request.Value)), context.CancellationToken);
		return new UserIdsDto
		{
			Ids =
			{
				followers.Select(x => x.Value.ToString()),
			},
		};
	}
}