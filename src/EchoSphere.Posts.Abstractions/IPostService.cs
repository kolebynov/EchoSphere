using EchoSphere.Posts.Abstractions.Models;
using EchoSphere.Users.Abstractions.Models;

namespace EchoSphere.Posts.Abstractions;

public interface IPostService
{
	ValueTask<PostId> PublishPost(UserId userId, string title, string body, CancellationToken cancellationToken);

	ValueTask<IReadOnlyList<Post>> GetUserPosts(UserId userId, CancellationToken cancellationToken);
}