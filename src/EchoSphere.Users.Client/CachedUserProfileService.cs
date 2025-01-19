using EchoSphere.Domain.Abstractions.Models;
using EchoSphere.Users.Abstractions;
using EchoSphere.Users.Abstractions.Models;
using LanguageExt.UnsafeValueAccess;
using ZiggyCreatures.Caching.Fusion;

namespace EchoSphere.Users.Client;

internal sealed class CachedUserProfileService : IUserProfileService
{
	private const string CacheVersion = "v1";

	private readonly IUserProfileService _inner;
	private readonly IFusionCache _cache;

	public CachedUserProfileService(IUserProfileService inner, IFusionCache cache)
	{
		_inner = inner;
		_cache = cache;
	}

	public Task<IReadOnlyList<UserProfile>> GetUserProfiles(CancellationToken cancellationToken) =>
		_inner.GetUserProfiles(cancellationToken);

	public Task<Option<UserProfile>> GetUserProfile(UserId userId, CancellationToken cancellationToken) =>
		_inner.GetUserProfile(userId, cancellationToken);

	public async Task<Option<BasicUserProfile>> GetBasicUserProfile(UserId userId, CancellationToken cancellationToken)
	{
		var result = await _cache.GetOrSetAsync(
			$"{CacheVersion}/UserProfileService/BasicUserProfile/{userId.Value}",
			async ct => (await _inner.GetBasicUserProfile(userId, ct)).ValueUnsafe(),
			token: cancellationToken);
		return Optional(result);
	}

	public Task<IReadOnlyList<(UserId UserId, bool Exists)>> CheckUsersExistence(
		IReadOnlyList<UserId> userIds, CancellationToken cancellationToken) =>
		_inner.CheckUsersExistence(userIds, cancellationToken);
}