namespace EchoSphere.Users.Abstractions.Extensions;

public static class FunctionalExtensions
{
	public static Task<Either<TLeft, TRight>> DoAsync<TLeft, TRight>(
		this Task<Either<TLeft, TRight>> eitherAsync, Func<TRight, Task> doAction) =>
		eitherAsync.MapAsync(async right =>
		{
			await doAction(right);
			return right;
		});
}