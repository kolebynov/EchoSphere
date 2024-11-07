using EchoSphere.Domain.Abstractions;
using EchoSphere.Domain.Abstractions.Models;
using EchoSphere.Infrastructure.IntegrationEvents.Abstractions;
using EchoSphere.Posts.Abstractions;
using EchoSphere.Posts.Abstractions.IntegrationEvents;
using EchoSphere.Posts.Abstractions.Models;
using EchoSphere.Posts.Api.Data.Models;
using LinqToDB;
using LinqToDB.Data;
using Microsoft.AspNetCore.Authorization;

namespace EchoSphere.Posts.Api.Services;

[Authorize]
internal sealed class PostService : IPostService
{
	private readonly DataConnection _dataConnection;
	private readonly ICurrentUserAccessor _currentUserAccessor;
	private readonly IIntegrationEventService _integrationEventService;

	public PostService(DataConnection dataConnection, ICurrentUserAccessor currentUserAccessor,
		IIntegrationEventService integrationEventService)
	{
		_dataConnection = dataConnection;
		_currentUserAccessor = currentUserAccessor;
		_integrationEventService = integrationEventService;
	}

	public async Task<Either<PublishPostError, PostId>> PublishPost(string body, CancellationToken cancellationToken)
	{
		var postId = new PostId(Guid.NewGuid());
		await using var transaction = await _dataConnection.BeginTransactionAsync(cancellationToken);

		await _dataConnection.InsertAsync(
			new PostDb { Id = postId, PostedOn = DateTimeOffset.UtcNow , AuthorId = _currentUserAccessor.CurrentUserId, Body = body },
			token: cancellationToken);

		await _integrationEventService.PublishEvent(
			new PostPublished
			{
				PostId = postId,
				UserId = _currentUserAccessor.CurrentUserId,
			},
			cancellationToken);

		await transaction.CommitAsync(cancellationToken);

		return postId;
	}

	public async Task<Option<IReadOnlyList<Post>>> GetUserPosts(UserId userId, CancellationToken cancellationToken)
	{
		var currentUserId = _currentUserAccessor.CurrentUserId;
		var postLikeTable = _dataConnection.GetTable<PostLikeDb>();
		var postCommentTable = _dataConnection.GetTable<PostCommentDb>();

		return await _dataConnection.GetTable<PostDb>()
			.Where(x => x.AuthorId == userId)
			.OrderByDescending(x => x.PostedOn)
			.LeftJoin(
				postLikeTable,
				(post, postLike) => post.Id == postLike.PostId && postLike.UserId == currentUserId,
				(post, postLike) => new Post
				{
					Id = post.Id,
					PostedOn = post.PostedOn,
					AuthorId = post.AuthorId,
					Body = post.Body,
					LikedByCurrentUser = postLike != null,
					LikesCount = postLikeTable.Count(x => x.PostId == post.Id),
					CommentsCount = postCommentTable.Count(x => x.PostId == post.Id),
				})
			.ToArrayAsync(cancellationToken);
	}

	public Task<Either<TogglePostLikeError, Unit>> TogglePostLike(PostId postId, CancellationToken cancellationToken) =>
		GetPost(postId, cancellationToken)
			.MapAsync(async post =>
			{
				await using var transaction = await _dataConnection.BeginTransactionAsync(cancellationToken);
				await TogglePostLikeInternal(post, cancellationToken);
				await transaction.CommitAsync(cancellationToken);

				return Either<TogglePostLikeError, Unit>.Right(Unit.Default);
			})
			.IfNone(TogglePostLikeError.PostNotFound());

	public async Task<Option<IReadOnlyList<PostComment>>> GetPostComments(PostId postId, CancellationToken cancellationToken)
	{
		if ((await GetPost(postId, cancellationToken)).IsNone)
		{
			return None;
		}

		return await _dataConnection.GetTable<PostCommentDb>()
			.Where(x => x.PostId == postId)
			.Select(x => new PostComment
			{
				Id = x.Id,
				Text = x.Text,
				UserId = x.UserId,
			})
			.ToArrayAsync(cancellationToken);
	}

	public Task<Either<AddCommentError, PostCommentId>> AddComment(
		PostId postId, string text, CancellationToken cancellationToken) =>
		GetPost(postId, cancellationToken)
			.MapAsync(async post =>
			{
				await using var transaction = await _dataConnection.BeginTransactionAsync(cancellationToken);

				var postCommentId = new PostCommentId(Guid.NewGuid());
				await _dataConnection.InsertAsync(
					new PostCommentDb
					{
						Id = postCommentId,
						Text = text,
						PostId = postId,
						UserId = _currentUserAccessor.CurrentUserId,
					}, token: cancellationToken);

				await _integrationEventService.PublishEvent(
					new PostCommentAdded
					{
						PostCommentId = postCommentId,
						PostId = postId,
						PostAuthorId = post.AuthorId,
						UserId = _currentUserAccessor.CurrentUserId,
					}, cancellationToken);

				await transaction.CommitAsync(cancellationToken);
				return Either<AddCommentError, PostCommentId>.Right(postCommentId);
			})
			.IfNone(AddCommentError.PostNotFound());

	private async Task TogglePostLikeInternal(PostDb post, CancellationToken cancellationToken)
	{
		var deleted = await _dataConnection.GetTable<PostLikeDb>()
			.DeleteAsync(x => x.PostId == post.Id && x.UserId == _currentUserAccessor.CurrentUserId, cancellationToken);
		if (deleted > 0)
		{
			return;
		}

		await _dataConnection.InsertAsync(
			new PostLikeDb
			{
				PostId = post.Id,
				UserId = _currentUserAccessor.CurrentUserId,
			}, token: cancellationToken);

		await _integrationEventService.PublishEvent(
			new PostLiked
			{
				PostId = post.Id,
				PostAuthorId = post.AuthorId,
				UserId = _currentUserAccessor.CurrentUserId,
			},
			cancellationToken);
	}

	private Task<Option<PostDb>> GetPost(PostId postId, CancellationToken cancellationToken) =>
		_dataConnection.GetTable<PostDb>().FirstOrDefaultAsync(x => x.Id == postId, cancellationToken).Map(Optional);
}