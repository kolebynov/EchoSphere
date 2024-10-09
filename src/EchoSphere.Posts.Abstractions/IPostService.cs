using Dusharp;
using EchoSphere.Domain.Abstractions.Models;
using EchoSphere.Posts.Abstractions.Models;
using LanguageExt;

namespace EchoSphere.Posts.Abstractions;

[Union]
public partial struct PublishPostError
{
	[UnionCase]
	public static partial PublishPostError InvalidUser();
}

public interface IPostService
{
	Task<Either<PublishPostError, PostId>> PublishPost(string title, string body,
		CancellationToken cancellationToken);

	Task<Option<IReadOnlyList<Post>>> GetUserPosts(UserId userId, CancellationToken cancellationToken);
}