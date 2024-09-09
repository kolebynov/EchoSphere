using EchoSphere.Posts.Abstractions;
using EchoSphere.Posts.Abstractions.Models;
using EchoSphere.Posts.Api.Data.Models;
using EchoSphere.Users.Abstractions.Models;
using LinqToDB;
using LinqToDB.Data;

namespace EchoSphere.Posts.Api.Services;

internal sealed class PostService : IPostService
{
	private readonly DataConnection _dataConnection;

	public PostService(DataConnection dataConnection)
	{
		_dataConnection = dataConnection ?? throw new ArgumentNullException(nameof(dataConnection));
	}

	public async ValueTask<PostId> PublishPost(UserId userId, string title, string body, CancellationToken cancellationToken)
	{
		var postId = new PostId(Guid.NewGuid());
		await _dataConnection.InsertAsync(
			new PostDb { Id = postId, UserId = userId, Title = title, Body = body }, token: cancellationToken);
		return postId;
	}

	public async ValueTask<IReadOnlyList<Post>> GetUserPosts(UserId userId, CancellationToken cancellationToken)
	{
		return await _dataConnection.GetTable<PostDb>()
			.Where(x => x.UserId == userId)
			.Select(x => new Post
			{
				Id = x.Id,
				Title = x.Title,
				Body = x.Body,
			})
			.ToArrayAsync(cancellationToken);
	}
}