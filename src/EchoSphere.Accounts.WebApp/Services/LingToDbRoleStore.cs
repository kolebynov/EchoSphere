using LinqToDB;

namespace EchoSphere.Accounts.WebApp.Services;

internal sealed class LingToDbRoleStore<TRole, TKey, TUserRole, TRoleClaim> :
	RoleStoreBase<TRole, TKey, TUserRole, TRoleClaim>
	where TRole : IdentityRole<TKey>
	where TKey : IEquatable<TKey>
	where TUserRole : IdentityUserRole<TKey>, new()
	where TRoleClaim : IdentityRoleClaim<TKey>, new()
{
	private readonly IDataContext _dataContext;

	public override IQueryable<TRole> Roles => _dataContext.GetTable<TRole>();

	public LingToDbRoleStore(IDataContext dataContext, IdentityErrorDescriber? describer = null)
		: base(describer ?? new IdentityErrorDescriber())
	{
		_dataContext = dataContext;
	}

	public override async Task<IdentityResult> CreateAsync(TRole role, CancellationToken cancellationToken = default)
	{
		await _dataContext.InsertAsync(role, token: cancellationToken);
		return IdentityResult.Success;
	}

	public override async Task<IdentityResult> UpdateAsync(
		TRole role, CancellationToken cancellationToken = default)
	{
		var result = await _dataContext.UpdateAsync(role, token: cancellationToken);
		return result == 1 ? IdentityResult.Success : IdentityResult.Failed(ErrorDescriber.ConcurrencyFailure());
	}

	public override async Task<IdentityResult> DeleteAsync(
		TRole role, CancellationToken cancellationToken = default)
	{
		var result = await _dataContext.GetTable<TRole>()
			.Where(_ => _.Id.Equals(role.Id) && _.ConcurrencyStamp == role.ConcurrencyStamp)
			.DeleteAsync(cancellationToken);

		return result == 1 ? IdentityResult.Success : IdentityResult.Failed(ErrorDescriber.ConcurrencyFailure());
	}

	public override Task<TRole?> FindByIdAsync(string id, CancellationToken cancellationToken = default)
	{
		var roleId = ConvertIdFromString(id);
		return _dataContext.GetTable<TRole>().FirstOrDefaultAsync(u => u.Id.Equals(roleId), cancellationToken);
	}

	public override Task<TRole?> FindByNameAsync(
		string normalizedName, CancellationToken cancellationToken = default)
	{
		return _dataContext.GetTable<TRole>()
			.FirstOrDefaultAsync(r => r.NormalizedName == normalizedName, cancellationToken);
	}

	public override async Task<IList<Claim>> GetClaimsAsync(
		TRole role, CancellationToken cancellationToken = default)
	{
		return await _dataContext.GetTable<TRoleClaim>()
			.Where(rc => rc.RoleId.Equals(role.Id))
			.Select(c => c.ToClaim())
			.ToListAsync(cancellationToken);
	}

	public override Task AddClaimAsync(TRole role, Claim claim, CancellationToken cancellationToken = default)
	{
		return _dataContext.InsertAsync(CreateRoleClaim(role, claim), token: cancellationToken);
	}

	public override Task RemoveClaimAsync(TRole role, Claim claim,
		CancellationToken cancellationToken = default)
	{
		return _dataContext.GetTable<TRoleClaim>()
			.Where(rc =>
				rc.RoleId.Equals(role.Id) && rc.ClaimValue == claim.Value && rc.ClaimType == claim.Type)
			.DeleteAsync(cancellationToken);
	}
}