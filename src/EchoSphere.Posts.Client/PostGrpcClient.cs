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
				PublishPostErrorDto.ErrorOneofCase.InvalidUser => PublishPostError.InvalidUser(),
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
						})
						.ToArray();
				})
			.IfLeft(_ => None);
}