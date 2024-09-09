using EchoSphere.GrpcModels;
using EchoSphere.Users.Abstractions;
using EchoSphere.Users.Abstractions.Models;
using EchoSphere.Users.Grpc;
using Google.Protobuf.WellKnownTypes;

namespace EchoSphere.Users.Client;

public sealed class UserProfileGrpcClient : IUserProfileService
{
	private readonly UserProfileService.UserProfileServiceClient _serviceGrpcClient;

	public UserProfileGrpcClient(UserProfileService.UserProfileServiceClient serviceGrpcClient)
	{
		_serviceGrpcClient = serviceGrpcClient;
	}

	public async ValueTask<IReadOnlyList<UserProfile>> GetUserProfiles(CancellationToken cancellationToken)
	{
		var profiles = await _serviceGrpcClient.GetUserProfilesAsync(new Empty(), cancellationToken: cancellationToken);
		return profiles.Profiles
			.Select(x => new UserProfile
			{
				Id = new UserId(Guid.Parse(x.Id)),
				FirstName = x.FirstName,
				SecondName = x.SecondName,
			})
			.ToArray();
	}

	public async ValueTask<UserProfile> GetUserProfile(UserId userId, CancellationToken cancellationToken)
	{
		var profile = await _serviceGrpcClient.GetUserProfileAsync(
			new UserIdDto { Value = userId.Value.ToString() }, cancellationToken: cancellationToken);
		return new UserProfile
		{
			Id = new UserId(Guid.Parse(profile.Id)),
			FirstName = profile.FirstName,
			SecondName = profile.SecondName,
		};
	}
}