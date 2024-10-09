using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Google.Rpc;
using Grpc.Core;

namespace EchoSphere.GrpcShared.Extensions;

public static class GrpcExceptionExtensions
{
	public static RpcException ToStatusRpcException<T>(this T details, Code code = Code.Unknown, string? message = null)
		where T : IMessage => new Google.Rpc.Status
	{
		Code = (int)code,
		Message = message ?? "error",
		Details = { Any.Pack(details) },
	}.ToRpcException();
}