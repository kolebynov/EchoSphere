using Microsoft.AspNetCore.Http.HttpResults;

namespace EchoSphere.ApiGateway.Extensions;

public static class ResultsExtensions
{
	public static Results<TResult1, TResult2> ToResults<TResult1, TResult2>(this TResult1 result)
		where TResult1 : IResult
		where TResult2 : IResult
		=> result;

	public static Results<TResult1, TResult2> ToResults<TResult1, TResult2>(this TResult2 result)
		where TResult1 : IResult
		where TResult2 : IResult
		=> result;
}