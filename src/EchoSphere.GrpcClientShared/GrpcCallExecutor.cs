using Google.Protobuf;
using Grpc.Core;
using LanguageExt;

namespace EchoSphere.GrpcClientShared;

public sealed class GrpcCallExecutor<TClient>
	where TClient : class
{
	private readonly TClient _client;

	public GrpcCallExecutor(TClient client)
	{
		_client = client;
	}

	public Task<TResult> ExecuteAsync<TResult>(Func<TClient, Task<TResult>> executeFunc) => executeFunc(_client);

	public async Task<Either<TError, TResult>> ExecuteAsync<TResult, TError>(Func<TClient, Task<TResult>> executeFunc)
		where TError : class, IMessage<TError>, new()
	{
		try
		{
			return await executeFunc(_client);
		}
		catch (RpcException e)
		{
			var rpcStatus = e.GetRpcStatus();
			if (rpcStatus == null)
			{
				throw;
			}

			var error = rpcStatus.GetDetail<TError>();
			if (error == null)
			{
				throw;
			}

			return error;
		}
	}
}