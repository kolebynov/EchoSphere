using EchoSphere.Domain.Abstractions.Extensions;
using EchoSphere.Domain.Abstractions.Models;
using EchoSphere.GrpcClientShared;
using EchoSphere.GrpcShared.Errors;
using EchoSphere.GrpcShared.Extensions;
using EchoSphere.Users.Abstractions;
using EchoSphere.Users.Abstractions.Extensions;
using EchoSphere.Users.Grpc;

namespace EchoSphere.Users.Client;

internal sealed class FollowGrpcClient : IFollowService
{
	private readonly GrpcCallExecutor<FollowService.FollowServiceClient> _grpcExecutor;

	public FollowGrpcClient(GrpcCallExecutor<FollowService.FollowServiceClient> grpcExecutor)
	{
		_grpcExecutor = grpcExecutor;
	}

	public Task<Either<FollowError, Unit>> Follow(UserId followerUserId, UserId followUserId,
		CancellationToken cancellationToken) =>
		_grpcExecutor
			.ExecuteAsync<Unit, FollowErrorDto>(
				async client =>
				{
					await client.FollowAsync(
						new FollowRequest
						{
							FollowerUserId = followerUserId.ToInnerString(),
							FollowUserId = followUserId.ToInnerString(),
						}, cancellationToken: cancellationToken);
					return Unit.Default;
				})
			.MapLeftAsync(err => err.ErrorCase switch
			{
				FollowErrorDto.ErrorOneofCase.CurrentUserNotFound => FollowError.FollowerUserNotFound(),
				FollowErrorDto.ErrorOneofCase.FollowUserNotFound => FollowError.FollowUserNotFound(),
				FollowErrorDto.ErrorOneofCase.AlreadyFollowed => FollowError.AlreadyFollowed(),
				_ => throw new ArgumentOutOfRangeException(nameof(err), err.ErrorCase, null),
			});

	public Task<Option<IReadOnlyList<UserId>>> GetFollowers(UserId userId, CancellationToken cancellationToken) =>
		_grpcExecutor
			.ExecuteAsync<Option<IReadOnlyList<UserId>>, NotFoundError>(
				async client =>
				{
					var followers =
						await client.GetFollowersAsync(userId.ToDto(), cancellationToken: cancellationToken);
					return followers.Ids
						.Select(IdValueExtensions.Parse<UserId>)
						.ToArray();
				})
			.IfLeft(_ => None);
}