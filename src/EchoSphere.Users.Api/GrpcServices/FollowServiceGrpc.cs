using EchoSphere.GrpcModels;
using EchoSphere.SharedModels.Extensions;
using EchoSphere.Users.Abstractions;
using EchoSphere.Users.Abstractions.Models;
using EchoSphere.Users.Api.Extensions;
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

	public override Task<Empty> Follow(FromToUserIds request, ServerCallContext context) =>
		_followService
			.Follow(
				IdValueExtensions.Parse<UserId>(request.FromUserId), IdValueExtensions.Parse<UserId>(request.ToUserId),
				context.CancellationToken)
			.Map(either => either
				.Map(_ => new Empty())
				.IfLeft(err => throw err.ToRpcException()));

	public override Task<UserIdsDto> GetFollowers(UserIdDto request, ServerCallContext context) =>
		_followService.GetFollowers(IdValueExtensions.Parse<UserId>(request.Value), context.CancellationToken)
			.Map(followersOpt => followersOpt
				.Map(followers => new UserIdsDto
				{
					Ids =
					{
						followers.Select(x => x.ToInnerString()),
					},
				})
				.IfNone(() => throw new RpcException(new Status(StatusCode.NotFound, "User not found."))));
}