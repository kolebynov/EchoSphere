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

	public static async Task<TRight> IfLeft<TLeft, TRight>(
		this Task<Either<TLeft, TRight>> eitherAsync, Func<TLeft, TRight> ifLeftFunc)
	{
		var either = await eitherAsync;
		return either.IfLeft(ifLeftFunc);
	}

	public static async Task<Either<TLeftRet, TRight>> MapLeftAsync<TLeft, TRight, TLeftRet>(
		this Task<Either<TLeft, TRight>> eitherAsync, Func<TLeft, TLeftRet> mapper)
	{
		var either = await eitherAsync;
		return either.MapLeft(mapper);
	}
}