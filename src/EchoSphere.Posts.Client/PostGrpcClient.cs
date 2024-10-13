using EchoSphere.Domain.Abstractions.Extensions;
using EchoSphere.Domain.Abstractions.Models;
using EchoSphere.GrpcClientShared;
using EchoSphere.GrpcShared.Errors;
using EchoSphere.GrpcShared.Extensions;
using EchoSphere.Posts.Abstractions;
using EchoSphere.Posts.Abstractions.Models;
using EchoSphere.Posts.Grpc;
using EchoSphere.Users.Abstractions.Extensions;

namespace EchoSphere.Posts.Client;

internal sealed class PostGrpcClient : IPostService
{
	private readonly GrpcCallExecutor<PostService.PostServiceClient> _grpcExecutor;

	public PostGrpcClient(GrpcCallExecutor<PostService.PostServiceClient> grpcExecutor)
	{
		_grpcExecutor = grpcExecutor;
	}

	public Task<Either<PublishPostError, PostId>> PublishPost(string title, string body,
		CancellationToken cancellationToken) =>
		_grpcExecutor
			.ExecuteAsync<PostId, PublishPostErrorDto>(
				async client =>
				{
					var postId = await client.PublishPostAsync(
						new PublishPostRequest
						{
							Title = title,
							Body = body,
						},
						cancellationToken: cancellationToken);
					return postId.ToModel();
				})
			.MapLeftAsync(err => err.ErrorCase switch
			{
				PublishPostErrorDto.ErrorOneofCase.CurrentUserNotFound => PublishPostError.CurrentUserNotFound(),
				_ => throw new ArgumentOutOfRangeException(nameof(err), err.ErrorCase, null),
			});

	public Task<Option<IReadOnlyList<Post>>> GetUserPosts(UserId userId, CancellationToken cancellationToken) =>
		_grpcExecutor
			.ExecuteAsync<Option<IReadOnlyList<Post>>, NotFoundError>(
				async client =>
				{
					var posts = await client.GetUserPostsAsync(userId.ToDto(), cancellationToken: cancellationToken);
					return posts.Posts
						.Select(x => new Post
						{
							Id = IdValueExtensions.Parse<PostId>(x.Id),
							Title = x.Title,
							Body = x.Body,
							LikedByCurrentUser = x.LikedByCurrentUser,
							LikesCount = x.LikesCount,
						})
						.ToArray();
				})
			.IfLeft(_ => None);

	public Task<Either<TogglePostLikeError, Unit>> TogglePostLike(PostId postId, CancellationToken cancellationToken) =>
		_grpcExecutor
			.ExecuteAsync<Unit, TogglePostLikeErrorDto>(
				async client =>
				{
					await client.TogglePostLikeAsync(postId.ToDto(), cancellationToken: cancellationToken);
					return Unit.Default;
				})
			.MapLeftAsync(err => err.ErrorCase switch
			{
				TogglePostLikeErrorDto.ErrorOneofCase.PostNotFound => TogglePostLikeError.PostNotFound(),
				_ => throw new ArgumentOutOfRangeException(nameof(err), err.ErrorCase, null),
			});

	public Task<Option<IReadOnlyList<PostComment>>> GetPostComments(PostId postId, CancellationToken cancellationToken) =>
		_grpcExecutor
			.ExecuteAsync<Option<IReadOnlyList<PostComment>>, NotFoundError>(
				async client =>
				{
					var comments = await client.GetPostCommentsAsync(postId.ToDto(), cancellationToken: cancellationToken);
					return comments.Comments
						.Select(x => new PostComment
						{
							Id = IdValueExtensions.Parse<PostCommentId>(x.Id),
							Text = x.Text,
							UserId = IdValueExtensions.Parse<UserId>(x.UserId),
						})
						.ToArray();
				})
			.IfLeft(_ => None);

	public Task<Either<AddCommentError, PostCommentId>> AddComment(PostId postId, string text, CancellationToken cancellationToken) =>
		_grpcExecutor
			.ExecuteAsync<PostCommentId, AddCommentErrorDto>(
				async client =>
				{
					var commentId = await client.AddCommentAsync(
						new AddCommentRequest
						{
							PostId = postId.ToInnerString(),
							Text = text,
						}, cancellationToken: cancellationToken);
					return IdValueExtensions.Parse<PostCommentId>(commentId.Value);
				})
			.MapLeftAsync(err => err.ErrorCase switch
			{
				AddCommentErrorDto.ErrorOneofCase.PostNotFound => AddCommentError.PostNotFound(),
				_ => throw new ArgumentOutOfRangeException(nameof(err), err.ErrorCase, null),
			});
}