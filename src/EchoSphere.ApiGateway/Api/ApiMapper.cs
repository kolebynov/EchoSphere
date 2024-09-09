namespace EchoSphere.ApiGateway.Api;

public static class ApiMapper
{
	public static IEndpointRouteBuilder MapApi(this IEndpointRouteBuilder routeBuilder)
	{
		routeBuilder.MapGroup("/api")
			.MapChatsApi()
			.MapUsersApi()
			.MapPostsApi();
		return routeBuilder;
	}
}