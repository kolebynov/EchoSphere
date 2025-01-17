﻿using EchoSphere.BlazorShared.Extensions;

namespace EchoSphere.Web.Client.Extensions;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddCommonClientAndServerServices(this IServiceCollection services)
	{
		services.AddSharedBlazor();

		return services;
	}
}