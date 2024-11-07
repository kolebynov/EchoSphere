using Dusharp;
using EchoSphere.Domain.Abstractions.Models;
using EchoSphere.Posts.Abstractions.Models;
using LanguageExt;

namespace EchoSphere.Posts.Abstractions;

[Union]
public partial struct PublishPostError
{
	[UnionCase]
	public static partial PublishPostError CurrentUserNotFound();
}

[Union]
public partial struct TogglePostLikeError
{
	[UnionCase]
	public static partial TogglePostLikeError PostNotFound();
}

[Union]
public partial struct AddCommentError
{
	[UnionCase]
	public static partial AddCommentError PostNotFound();
}

public interface IPostService
{
	Task<Either<PublishPostError, PostId>> PublishPost(string body,
		CancellationToken cancellationToken);

	Task<Option<IReadOnlyList<Post>>> GetUserPosts(UserId userId, CancellationToken cancellationToken);

	Task<Either<TogglePostLikeError, Unit>> TogglePostLike(PostId postId, CancellationToken cancellationToken);

	Task<Option<IReadOnlyList<PostComment>>> GetPostComments(PostId postId, CancellationToken cancellationToken);

	Task<Either<AddCommentError, PostCommentId>> AddComment(PostId postId, string text, CancellationToken cancellationToken);
}