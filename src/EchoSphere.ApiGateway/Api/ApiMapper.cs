namespace EchoSphere.ApiGateway.Api;

public static class ApiMapper
{
	public static IEndpointRouteBuilder MapApi(this IEndpointRouteBuilder routeBuilder)
	{
		routeBuilder.MapGroup("/api")
			.MapChatApi()
			.MapUserApi();
		return routeBuilder;
	}
}