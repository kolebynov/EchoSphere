using EchoSphere.GrpcClientShared.Extensions;
using EchoSphere.Users.Abstractions;
using EchoSphere.Users.Grpc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EchoSphere.Users.Client.Extensions;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddUsersGrpcClient(this IServiceCollection services, Uri address)
	{
		services
			.AddGrpcClientWithExecutor<UserProfileService.UserProfileServiceClient>(o => o.Address = address)
			.AddGrpcClientWithExecutor<FriendService.FriendServiceClient>(o => o.Address = address)
			.AddGrpcClientWithExecutor<FollowService.FollowServiceClient>(o => o.Address = address);

		services.TryAddScoped<IFriendService, FriendGrpcClient>();
		services.TryAddScoped<IFollowService, FollowGrpcClient>();
		services.TryAddScoped<IUserProfileService, UserProfileGrpcClient>();

		return services;
	}
}