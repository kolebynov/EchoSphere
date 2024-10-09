using EchoSphere.Domain.Abstractions.Models;
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

	public async Task<IReadOnlyList<UserProfile>> GetUserProfiles(CancellationToken cancellationToken) =>
		await _dataContext.GetTable<UserProfile>().ToArrayAsync(cancellationToken);

	public Task<Option<UserProfile>> GetUserProfile(UserId userId, CancellationToken cancellationToken) =>
		_dataContext.GetTable<UserProfile>().FirstOrDefaultAsync(x => x.Id == userId, cancellationToken).Map(Optional);

	public async Task<IReadOnlyList<(UserId UserId, bool Exists)>> CheckUsersExistence(
		IReadOnlyList<UserId> userIds, CancellationToken cancellationToken)
	{
		var foundUsers = await _dataContext.GetTable<UserProfile>()
			.Where(x => userIds.Contains(x.Id))
			.Select(x => x.Id)
			.AsAsyncEnumerable()
			.ToHashSetAsync(cancellationToken);

		return userIds
			.Select(x => (x, foundUsers.Contains(x)))
			.ToArray();
	}
}