using EchoSphere.Users.Abstractions;
using EchoSphere.Users.Abstractions.Models;
using LinqToDB;

namespace EchoSphere.Users.Api.Services;

internal sealed class UserProfileService : IUserProfileService
{
	private readonly IDataContext _dataContext;

	public UserProfileService(IDataContext dataContext)
	{
		_dataContext = dataContext;
	}

	public async ValueTask<IReadOnlyList<UserProfile>> GetUserProfiles(CancellationToken cancellationToken)
	{
		return await _dataContext.GetTable<UserProfile>()
			.ToArrayAsync(cancellationToken);
	}

	public ValueTask<UserProfile> GetUserProfile(UserId userId, CancellationToken cancellationToken) =>
		new(_dataContext.GetTable<UserProfile>().FirstAsync(x => x.Id == userId, cancellationToken));
}