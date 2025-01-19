using EchoSphere.Domain.Abstractions.Models;
using EchoSphere.Users.Abstractions;
using EchoSphere.Users.Abstractions.Models;
using LinqToDB;

namespace EchoSphere.Users.Api.Services;

internal sealed class UserProfileService : IUserProfileService
{
	private readonly IDataContext _dataContext;
	private readonly ITable<UserProfile> _userProfilesTable;

	public UserProfileService(IDataContext dataContext)
	{
		_dataContext = dataContext;
		_userProfilesTable  = dataContext.GetTable<UserProfile>();
	}

	public async Task<IReadOnlyList<UserProfile>> GetUserProfiles(CancellationToken cancellationToken) =>
		await _userProfilesTable.ToArrayAsync(cancellationToken);

	public Task<Option<UserProfile>> GetUserProfile(UserId userId, CancellationToken cancellationToken) =>
		_userProfilesTable.FirstOrDefaultAsync(x => x.Id == userId, cancellationToken).Map(Optional);

	public Task<Option<BasicUserProfile>> GetBasicUserProfile(UserId userId, CancellationToken cancellationToken) =>
		_userProfilesTable.Where(x => x.Id == userId)
			.Select(x => new BasicUserProfile
			{
				Id = x.Id,
				FirstName = x.FirstName,
				SecondName = x.SecondName,
			})
			.FirstOrDefaultAsync(cancellationToken)
			.Map(Optional);

	public async Task<IReadOnlyList<(UserId UserId, bool Exists)>> CheckUsersExistence(
		IReadOnlyList<UserId> userIds, CancellationToken cancellationToken)
	{
		var foundUsers = await _userProfilesTable
			.Where(x => userIds.Contains(x.Id))
			.Select(x => x.Id)
			.AsAsyncEnumerable()
			.ToHashSetAsync(cancellationToken);

		return userIds
			.Select(x => (x, foundUsers.Contains(x)))
			.ToArray();
	}
}