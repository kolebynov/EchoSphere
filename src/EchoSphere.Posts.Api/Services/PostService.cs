using EchoSphere.Domain.Abstractions;
using EchoSphere.Domain.Abstractions.Models;
using EchoSphere.Posts.Abstractions;
using EchoSphere.Posts.Abstractions.Models;
using EchoSphere.Posts.Api.Data.Models;
using LanguageExt;
using LinqToDB;
using LinqToDB.Data;
using Microsoft.AspNetCore.Authorization;

namespace EchoSphere.Posts.Api.Services;

[Authorize]
internal sealed class PostService : IPostService
{
	private readonly DataConnection _dataConnection;
	private readonly ICurrentUserAccessor _currentUserAccessor;

	public PostService(DataConnection dataConnection, ICurrentUserAccessor currentUserAccessor)
	{
		_dataConnection = dataConnection ?? throw new ArgumentNullException(nameof(dataConnection));
		_currentUserAccessor = currentUserAccessor;
	}

	public async Task<Either<PublishPostError, PostId>> PublishPost(string title, string body,
		CancellationToken cancellationToken)
	{
		var postId = new PostId(Guid.NewGuid());
		await _dataConnection.InsertAsync(
			new PostDb { Id = postId, UserId = _currentUserAccessor.CurrentUserId, Title = title, Body = body },
			token: cancellationToken);
		return postId;
	}

	public async Task<Option<IReadOnlyList<Post>>> GetUserPosts(UserId userId, CancellationToken cancellationToken)
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