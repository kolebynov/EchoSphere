using EchoSphere.Domain.Abstractions.Extensions;
using EchoSphere.Domain.Abstractions.Models;
using EchoSphere.GrpcClientShared;
using EchoSphere.GrpcShared.Errors;
using EchoSphere.GrpcShared.Extensions;
using EchoSphere.Users.Abstractions;
using EchoSphere.Users.Abstractions.Extensions;
using EchoSphere.Users.Abstractions.Models;
using EchoSphere.Users.Grpc;

namespace EchoSphere.Users.Client;

internal sealed class FriendGrpcClient : IFriendService
{
	private readonly GrpcCallExecutor<FriendService.FriendServiceClient> _grpcExecutor;

	public FriendGrpcClient(GrpcCallExecutor<FriendService.FriendServiceClient> grpcExecutor)
	{
		_grpcExecutor = grpcExecutor;
	}

	public Task<Option<IReadOnlyList<UserId>>> GetFriends(UserId userId, CancellationToken cancellationToken) =>
		_grpcExecutor.ExecuteAsync<Option<IReadOnlyList<UserId>>, NotFoundError>(
			async client =>
			{
				var friends = await client.GetFriendsAsync(userId.ToDto(), cancellationToken: cancellationToken);
				return friends.Ids
					.Select(IdValueExtensions.Parse<UserId>)
					.ToArray();
			}).IfLeft(_ => None);

	public Task<Either<SendFriendInviteError, Unit>> SendFriendInvite(UserId toUserId, CancellationToken cancellationToken) =>
		_grpcExecutor
			.ExecuteAsync<Unit, SendFriendInviteErrorDto>(
				async client =>
				{
					await client.SendFriendInviteAsync(toUserId.ToDto(), cancellationToken: cancellationToken);
					return Unit.Default;
				})
			.MapLeftAsync(err => err.ErrorCase switch
			{
				SendFriendInviteErrorDto.ErrorOneofCase.CurrentUserNotFound => SendFriendInviteError.CurrentUserNotFound(),
				SendFriendInviteErrorDto.ErrorOneofCase.ToUserNotFound => SendFriendInviteError.ToUserNotFound(),
				SendFriendInviteErrorDto.ErrorOneofCase.AlreadySent => SendFriendInviteError.AlreadySent(),
				SendFriendInviteErrorDto.ErrorOneofCase.AlreadyFriend => SendFriendInviteError.AlreadyFriend(),
				_ => throw new ArgumentOutOfRangeException(nameof(err), err.ErrorCase, null),
			});

	public Task<Option<IReadOnlyList<FriendInvitation>>> GetCurrentUserFriendInvites(
		CancellationToken cancellationToken) =>
		_grpcExecutor
			.ExecuteAsync<Option<IReadOnlyList<FriendInvitation>>, NotFoundError>(
				async client =>
				{
					var invites = await client.GetCurrentUserFriendInvitesAsync(
						GrpcExtensions.EmptyInstance, cancellationToken: cancellationToken);

					return Some<IReadOnlyList<FriendInvitation>>(invites.Invitations
						.Select(x => new FriendInvitation
						{
							Id = IdValueExtensions.Parse<FriendInvitationId>(x.Id),
							FromUserId = IdValueExtensions.Parse<UserId>(x.FromUserId),
						})
						.ToArray());
				})
			.IfLeft(_ => None);

	public Task<Either<FriendInviteError, Unit>> AcceptFriendInvite(
		FriendInvitationId invitationId, CancellationToken cancellationToken) =>
		_grpcExecutor
			.ExecuteAsync<Unit, FriendInviteErrorDto>(
				async client =>
				{
					await client.AcceptFriendInviteAsync(
						new FriendInvitationIdDto { Value = invitationId.ToInnerString() },
						cancellationToken: cancellationToken);
					return Unit.Default;
				})
			.MapLeftAsync(err => err.ErrorCase switch
			{
				FriendInviteErrorDto.ErrorOneofCase.InvitationNotFound => FriendInviteError.InvitationNotFound(),
				_ => throw new ArgumentOutOfRangeException(nameof(err), err.ErrorCase, null),
			});

	public Task<Either<FriendInviteError, Unit>> RejectFriendInvite(
		FriendInvitationId invitationId, CancellationToken cancellationToken) =>
		_grpcExecutor
			.ExecuteAsync<Unit, FriendInviteErrorDto>(
				async client =>
				{
					await client.RejectFriendInviteAsync(
						new FriendInvitationIdDto { Value = invitationId.ToInnerString() },
						cancellationToken: cancellationToken);
					return Unit.Default;
				})
			.MapLeftAsync(err => err.ErrorCase switch
			{
				FriendInviteErrorDto.ErrorOneofCase.InvitationNotFound => FriendInviteError.InvitationNotFound(),
				_ => throw new ArgumentOutOfRangeException(nameof(err), err.ErrorCase, null),
			});
}