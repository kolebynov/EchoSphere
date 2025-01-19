using EchoSphere.Domain.Abstractions.Models;
using EchoSphere.Users.Abstractions;
using EchoSphere.Users.Abstractions.Models;
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

	public Task<Option<BasicUserProfile>> GetBasicUserProfile(UserId userId, CancellationToken cancellationToken) =>
		_cache.GetOrSetAsync(
			$"{CacheVersion}_UserProfileService_BasicUserProfile_{userId.Value}",
			ct => _inner.GetBasicUserProfile(userId, ct),
			token: cancellationToken).AsTask();

	public Task<IReadOnlyList<(UserId UserId, bool Exists)>> CheckUsersExistence(
		IReadOnlyList<UserId> userIds, CancellationToken cancellationToken) =>
		_inner.CheckUsersExistence(userIds, cancellationToken);
}