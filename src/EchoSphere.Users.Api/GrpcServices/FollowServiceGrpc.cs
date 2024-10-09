using EchoSphere.Domain.Abstractions.Extensions;
using EchoSphere.Domain.Abstractions.Models;
using EchoSphere.GrpcShared.Contracts;
using EchoSphere.GrpcShared.Errors;
using EchoSphere.GrpcShared.Extensions;
using EchoSphere.Users.Abstractions;
using EchoSphere.Users.Abstractions.Extensions;
using EchoSphere.Users.Grpc;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;

namespace EchoSphere.Users.Api.GrpcServices;

[Authorize]
internal sealed class FollowServiceGrpc : FollowService.FollowServiceBase
{
	private readonly IFollowService _followService;

	public FollowServiceGrpc(IFollowService followService)
	{
		_followService = followService;
	}

	public override Task<Empty> Follow(UserIdDto request, ServerCallContext context) =>
		_followService
			.Follow(IdValueExtensions.Parse<UserId>(request.Value), context.CancellationToken)
			.MapAsync(_ => GrpcExtensions.EmptyInstance)
			.IfLeft(err => throw err
				.Match(
					() => new FollowErrorDto { CurrentUserNotFound = GrpcExtensions.EmptyInstance },
					() => new FollowErrorDto { FollowUserNotFound = GrpcExtensions.EmptyInstance },
					() => new FollowErrorDto { AlreadyFollowed = GrpcExtensions.EmptyInstance })
				.ToStatusRpcException());

	public override Task<UserIdsDto> GetFollowers(UserIdDto request, ServerCallContext context) =>
		_followService.GetFollowers(request.ToModel(), context.CancellationToken)
			.MapAsync(followers => new UserIdsDto
			{
				Ids =
				{
					followers.Select(x => x.ToInnerString()),
				},
			})
			.IfNone(() => throw NotFoundError.Instance.ToStatusRpcException());
}