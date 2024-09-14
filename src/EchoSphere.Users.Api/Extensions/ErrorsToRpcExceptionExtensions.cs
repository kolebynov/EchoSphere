using EchoSphere.Users.Abstractions;
using Grpc.Core;

namespace EchoSphere.Users.Api.Extensions;

public static class ErrorsToRpcExceptionExtensions
{
	public static RpcException ToRpcException(this FollowError followError)
	{
		return new RpcException(new Status(StatusCode.InvalidArgument, "Follow error"));
	}
}