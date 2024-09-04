using EchoSphere.UsersApi.Grpc;
using EchoSphere.UsersApi.Models;
using Grpc.Core;
using LinqToDB;
using Microsoft.AspNetCore.Identity;

namespace EchoSphere.UsersApi.Services;

internal sealed class UsersService : Grpc.UsersService.UsersServiceBase
{
	private readonly ILogger<UsersService> _logger;
	private readonly UserManager<UserProfile> _userManager;

	public UsersService(ILogger<UsersService> logger, UserManager<UserProfile> userManager)
	{
		_logger = logger;
		_userManager = userManager;
	}

	public override async Task<CreateUserResponse> CreateUser(CreateUserRequest request, ServerCallContext context)
	{
		var userId = Guid.CreateVersion7();
		var identityResult = await _userManager.CreateAsync(
			new UserProfile
			{
				Id = userId,
				UserName = request.UserName,
				Email = request.Email,
			},
			request.Password);
		if (!identityResult.Succeeded)
		{
			throw new RpcException(new Status(StatusCode.InvalidArgument, identityResult.Errors.First().Description));
		}

		return new CreateUserResponse { Id = userId.ToString("D") };
	}

	public override async Task<GetUserProfilesResponse> GetUserProfiles(
		GetUserProfilesRequest request, ServerCallContext context)
	{
		var profiles = await _userManager.Users.ToArrayAsync(context.CancellationToken);
		var response = new GetUserProfilesResponse();
		response.Profiles.AddRange(profiles
			.Select(user => new UserProfileDto
			{
				UserName = user.UserName,
				Email = user.Email,
			}));

		return response;
	}

	public override async Task<GetUserByUserNameAndPasswordResponse> GetUserByUserNameAndPassword(
		GetUserByUserNameAndPasswordRequest request, ServerCallContext context)
	{
		var user = await _userManager.FindByNameAsync(request.UserName);
		if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
		{
			return new GetUserByUserNameAndPasswordResponse { User = null };
		}

		return new GetUserByUserNameAndPasswordResponse
		{
			User = new UserProfileDto
			{
				UserName = user.UserName,
				Email = user.Email,
			},
		};
	}
}