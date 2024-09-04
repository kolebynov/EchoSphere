using System.Linq.Expressions;
using System.Security.Claims;
using LinqToDB;
using LinqToDB.Data;
using Microsoft.AspNetCore.Identity;

namespace EchoSphere.UsersApi.Services;

public class LingToDbUserStore<TKey, TUser, TRole, TUserClaim, TUserRole, TUserLogin, TUserToken, TRoleClaim> :
	UserStoreBase<TUser, TRole, TKey, TUserClaim, TUserRole, TUserLogin, TUserToken, TRoleClaim>
	where TUser : IdentityUser<TKey>
	where TRole : IdentityRole<TKey>
	where TUserClaim : IdentityUserClaim<TKey>, new()
	where TUserRole : IdentityUserRole<TKey>, new()
	where TUserLogin : IdentityUserLogin<TKey>, new()
	where TUserToken : IdentityUserToken<TKey>, new()
	where TRoleClaim : IdentityRoleClaim<TKey>, new()
	where TKey : IEquatable<TKey>
{
	private readonly IDataContext _dataContext;

	public LingToDbUserStore(IDataContext dataContext, IdentityErrorDescriber? describer = null)
		: base(describer ?? new IdentityErrorDescriber())
	{
		_dataContext = dataContext;
	}

	public override IQueryable<TUser> Users => _dataContext.GetTable<TUser>();

	public override async Task<IList<Claim>> GetClaimsAsync(TUser user, CancellationToken cancellationToken = default)
	{
		return (await _dataContext
			.GetTable<TUserClaim>()
			.Where(uc => uc.UserId.Equals(user.Id))
			.ToArrayAsync(cancellationToken))
			.Select(c => c.ToClaim())
			.ToList();
	}

	public override Task AddClaimsAsync(TUser user, IEnumerable<Claim> claims,
		CancellationToken cancellationToken = default)
	{
		var data = claims.Select(_ => CreateUserClaim(user, _));
		return _dataContext.GetTable<TUserClaim>().BulkCopyAsync(data, cancellationToken);
	}

	public override Task ReplaceClaimAsync(TUser user, Claim claim, Claim newClaim,
		CancellationToken cancellationToken = default)
	{
		var q = _dataContext
			.GetTable<TUserClaim>()
			.Where(uc => uc.UserId.Equals(user.Id) && uc.ClaimValue == claim.Value && uc.ClaimType == claim.Type);

		return q
			.Set(_ => _.ClaimValue, newClaim.Value)
			.Set(_ => _.ClaimType, newClaim.Type)
			.UpdateAsync(cancellationToken);
	}

	public override Task RemoveClaimsAsync(TUser user, IEnumerable<Claim> claims,
		CancellationToken cancellationToken = default)
	{
		var q = _dataContext.GetTable<TUserClaim>();
		var userId = Expression.PropertyOrField(Expression.Constant(user, typeof(TUser)), nameof(user.Id));
		var equals = typeof(TKey).GetMethod(nameof(IEquatable<TKey>.Equals), [typeof(TKey)])!;
		var uc = Expression.Parameter(typeof(TUserClaim));
		Expression? body = null;
		var ucUserId = Expression.PropertyOrField(uc, nameof(IdentityUserClaim<TKey>.UserId));
		var userIdEquals = Expression.Call(ucUserId, @equals, userId);

		foreach (var claim in claims)
		{
			var cl = Expression.Constant(claim);

			var claimValueEquals = Expression.Equal(
				Expression.PropertyOrField(uc, nameof(IdentityUserClaim<TKey>.ClaimValue)),
				Expression.PropertyOrField(cl, nameof(Claim.Value)));
			var claimTypeEquals =
				Expression.Equal(
					Expression.PropertyOrField(uc, nameof(IdentityUserClaim<TKey>.ClaimType)),
					Expression.PropertyOrField(cl, nameof(Claim.Type)));

			var predicatePart = Expression.And(Expression.And(userIdEquals, claimValueEquals), claimTypeEquals);

			body = body == null ? predicatePart : Expression.Or(body, predicatePart);
		}

		if (body != null)
		{
			var predicate = Expression.Lambda<Func<TUserClaim, bool>>(body, uc);

			return q.Where(predicate).DeleteAsync(cancellationToken);
		}

		return Task.CompletedTask;
	}

	public override async Task<IList<TUser>> GetUsersForClaimAsync(
		Claim claim, CancellationToken cancellationToken = default)
	{
		var query = from userClaims in _dataContext.GetTable<TUserClaim>()
			join user in _dataContext.GetTable<TUser>() on userClaims.UserId equals user.Id
			where userClaims.ClaimValue == claim.Value && userClaims.ClaimType == claim.Type
			select user;

		return await query.ToListAsync(cancellationToken);
	}

	public override Task<TUser?> FindByEmailAsync(
		string normalizedEmail, CancellationToken cancellationToken = default)
	{
		return _dataContext.GetTable<TUser>()
			.FirstOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail, cancellationToken);
	}

	public override async Task<IdentityResult> CreateAsync(
		TUser user, CancellationToken cancellationToken = default)
	{
		await _dataContext.InsertAsync(user, token: cancellationToken);
		return IdentityResult.Success;
	}

	public override async Task<IdentityResult> UpdateAsync(
		TUser user, CancellationToken cancellationToken = default)
	{
		var result = await _dataContext.UpdateAsync(user, token: cancellationToken);
		return result == 1 ? IdentityResult.Success : IdentityResult.Failed(ErrorDescriber.ConcurrencyFailure());
	}

	public override async Task<IdentityResult> DeleteAsync(
		TUser user, CancellationToken cancellationToken = default)
	{
		var result = await _dataContext.GetTable<TUser>()
			.Where(_ => _.Id.Equals(user.Id) && _.ConcurrencyStamp == user.ConcurrencyStamp)
			.DeleteAsync(cancellationToken);
		return result == 1 ? IdentityResult.Success : IdentityResult.Failed(ErrorDescriber.ConcurrencyFailure());
	}

	public override Task<TUser?> FindByIdAsync(
		string userId, CancellationToken cancellationToken = default)
	{
		var id = ConvertIdFromString(userId)!;
		return FindUserAsync(id, cancellationToken);
	}

	public override Task<TUser?> FindByNameAsync(
		string normalizedUserName, CancellationToken cancellationToken = default)
	{
		return _dataContext.GetTable<TUser>()
			.FirstOrDefaultAsync(u => u.NormalizedUserName == normalizedUserName, cancellationToken);
	}

	public override Task AddLoginAsync(TUser user, UserLoginInfo login,
		CancellationToken cancellationToken = default)
	{
		return _dataContext.InsertAsync(CreateUserLogin(user, login), token: cancellationToken);
	}

	public override Task RemoveLoginAsync(TUser user, string loginProvider, string providerKey,
		CancellationToken cancellationToken = default)
	{
		return _dataContext.GetTable<TUserLogin>()
			.DeleteAsync(
				userLogin =>
					userLogin.UserId.Equals(user.Id) && userLogin.LoginProvider == loginProvider &&
					userLogin.ProviderKey == providerKey,
				cancellationToken);
	}

	public override async Task<IList<UserLoginInfo>> GetLoginsAsync(
		TUser user, CancellationToken cancellationToken = default)
	{
		var userId = user.Id;
		return await _dataContext
			.GetTable<TUserLogin>()
			.Where(l => l.UserId.Equals(userId))
			.Select(l => new UserLoginInfo(l.LoginProvider, l.ProviderKey, l.ProviderDisplayName))
			.ToListAsync(cancellationToken);
	}

	public override async Task AddToRoleAsync(TUser user, string normalizedRoleName,
		CancellationToken cancellationToken = default)
	{
		var roleEntity = await FindRoleAsync(normalizedRoleName, cancellationToken);
		if (roleEntity == null)
		{
			throw new InvalidOperationException($"Role {normalizedRoleName} does not exist.");
		}

		await _dataContext.InsertAsync(CreateUserRole(user, roleEntity), token: cancellationToken);
	}

	public override Task RemoveFromRoleAsync(TUser user, string normalizedRoleName,
		CancellationToken cancellationToken = default)
	{
		var q =
			from ur in _dataContext.GetTable<TUserRole>()
			join r in _dataContext.GetTable<TRole>() on ur.RoleId equals r.Id
			where r.NormalizedName == normalizedRoleName && ur.UserId.Equals(user.Id)
			select ur;

		return q.DeleteAsync(cancellationToken);
	}

	public override async Task<IList<string>> GetRolesAsync(
		TUser user, CancellationToken cancellationToken = default)
	{
		var userId = user.Id;
		var query = from userRole in _dataContext.GetTable<TUserRole>()
			join role in _dataContext.GetTable<TRole>() on userRole.RoleId equals role.Id
			where userRole.UserId.Equals(userId)
			select role.Name;

		return await query.ToListAsync(cancellationToken);
	}

	public override Task<bool> IsInRoleAsync(TUser user, string normalizedRoleName,
		CancellationToken cancellationToken = default)
	{
		var q = from ur in _dataContext.GetTable<TUserRole>()
			join r in _dataContext.GetTable<TRole>() on ur.RoleId equals r.Id
			where r.NormalizedName == normalizedRoleName && ur.UserId.Equals(user.Id)
			select ur;

		return q.AnyAsync(cancellationToken);
	}

	public override async Task<IList<TUser>> GetUsersInRoleAsync(
		string normalizedRoleName, CancellationToken cancellationToken = default)
	{
		var query = from userRole in _dataContext.GetTable<TUserRole>()
			join user in _dataContext.GetTable<TUser>() on userRole.UserId equals user.Id
			join role in _dataContext.GetTable<TRole>() on userRole.RoleId equals role.Id
			where role.NormalizedName == normalizedRoleName
			select user;

		return await query.ToListAsync(cancellationToken);
	}

	protected override Task RemoveUserTokenAsync(TUserToken token)
	{
		return _dataContext.GetTable<TUserToken>()
			.DeleteAsync(
				t => t.UserId.Equals(token.UserId) && t.LoginProvider == token.LoginProvider && t.Name == token.Name);
	}

	protected override Task<TUserToken?> FindTokenAsync(TUser user, string loginProvider, string name, CancellationToken cancellationToken)
	{
		return _dataContext
			.GetTable<TUserToken>()
			.Where(_ => _.UserId.Equals(user.Id) && _.LoginProvider == loginProvider && _.Name == name)
			.FirstOrDefaultAsync(cancellationToken);
	}

	protected override Task AddUserTokenAsync(TUserToken token)
	{
		return _dataContext.InsertAsync(token);
	}

	protected override Task<TUser?> FindUserAsync(TKey userId, CancellationToken cancellationToken)
	{
		return _dataContext.GetTable<TUser>()
			.FirstOrDefaultAsync(x => x.Id.Equals(userId), cancellationToken);
	}

	protected override Task<TUserLogin?> FindUserLoginAsync(TKey userId, string loginProvider, string providerKey, CancellationToken cancellationToken)
	{
		return _dataContext.GetTable<TUserLogin>()
			.SingleOrDefaultAsync(
				userLogin => userLogin.UserId.Equals(userId) && userLogin.LoginProvider == loginProvider &&
				             userLogin.ProviderKey == providerKey,
				cancellationToken);
	}

	protected override Task<TUserLogin?> FindUserLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken)
	{
		return _dataContext.GetTable<TUserLogin>()
			.SingleOrDefaultAsync(
				userLogin => userLogin.LoginProvider == loginProvider && userLogin.ProviderKey == providerKey,
				cancellationToken);
	}

	protected override Task<TRole?> FindRoleAsync(string normalizedRoleName, CancellationToken cancellationToken)
	{
		return _dataContext.GetTable<TRole>()
			.SingleOrDefaultAsync(r => r.NormalizedName == normalizedRoleName, cancellationToken);
	}

	protected override Task<TUserRole?> FindUserRoleAsync(TKey userId, TKey roleId, CancellationToken cancellationToken)
	{
		return _dataContext.GetTable<TUserRole>()
			.SingleOrDefaultAsync(
				userRole => userRole.UserId.Equals(userId) && userRole.RoleId.Equals(roleId),
				cancellationToken);
	}
}