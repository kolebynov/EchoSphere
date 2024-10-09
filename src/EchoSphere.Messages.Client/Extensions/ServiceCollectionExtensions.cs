using EchoSphere.GrpcClientShared.Extensions;
using EchoSphere.Messages.Abstractions;
using EchoSphere.Messages.Grpc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EchoSphere.Messages.Client.Extensions;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddMessagesGrpcClient(this IServiceCollection services, Uri address)
	{
		services.AddGrpcClientWithExecutor<ChatService.ChatServiceClient>(o => o.Address = address);

		services.TryAddScoped<IChatService, ChatGrpcClient>();

		return services;
	}
}