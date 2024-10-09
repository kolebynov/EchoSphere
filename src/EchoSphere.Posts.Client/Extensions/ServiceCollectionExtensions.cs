using EchoSphere.GrpcClientShared.Extensions;
using EchoSphere.Posts.Abstractions;
using EchoSphere.Posts.Grpc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EchoSphere.Posts.Client.Extensions;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddPostsGrpcClient(this IServiceCollection services, Uri address)
	{
		services.AddGrpcClientWithExecutor<PostService.PostServiceClient>(o => o.Address = address);

		services.TryAddScoped<IPostService, PostGrpcClient>();

		return services;
	}
}