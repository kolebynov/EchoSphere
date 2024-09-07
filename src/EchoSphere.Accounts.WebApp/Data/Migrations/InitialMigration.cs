using System.Data;
using EchoSphere.Accounts.WebApp.Models;
using FluentMigrator;

namespace EchoSphere.Accounts.WebApp.Data.Migrations;

[Migration(1)]
public sealed class InitialMigration : Migration
{
	public override void Up()
	{
		Create.Table(DataConstants.AccountsTableName)
			.WithColumn(nameof(Account.Id)).AsGuid().PrimaryKey().NotNullable()
			.WithColumn(nameof(Account.UserName)).AsString(60)
			.WithColumn(nameof(Account.NormalizedUserName)).AsString(60).Indexed()
			.WithColumn(nameof(Account.ConcurrencyStamp)).AsString()
			.WithColumn(nameof(Account.PasswordHash)).AsString()
			.WithColumn(nameof(Account.Email)).AsString(60)
			.WithColumn(nameof(Account.EmailConfirmed)).AsBoolean()
			.WithColumn(nameof(Account.LockoutEnabled)).AsBoolean()
			.WithColumn(nameof(Account.LockoutEnd)).AsDateTimeOffset().Nullable()
			.WithColumn(nameof(Account.NormalizedEmail)).AsString(60).Indexed()
			.WithColumn(nameof(Account.PhoneNumber)).AsString(20).Nullable()
			.WithColumn(nameof(Account.SecurityStamp)).AsString()
			.WithColumn(nameof(Account.AccessFailedCount)).AsInt32()
			.WithColumn(nameof(Account.PhoneNumberConfirmed)).AsBoolean()
			.WithColumn(nameof(Account.TwoFactorEnabled)).AsBoolean();

		Create.Table(DataConstants.AccountClaimsTableName)
			.WithColumn(nameof(IdentityUserClaim<Guid>.Id)).AsInt32().PrimaryKey()
			.WithColumn(nameof(IdentityUserClaim<Guid>.UserId)).AsGuid().Indexed()
				.ForeignKey(DataConstants.AccountsTableName, nameof(Account.Id)).OnDelete(Rule.Cascade)
			.WithColumn(nameof(IdentityUserClaim<Guid>.ClaimType)).AsString(32)
			.WithColumn(nameof(IdentityUserClaim<Guid>.ClaimValue)).AsString(1024);

		Create.Table(DataConstants.RolesTableName)
			.WithColumn(nameof(Role.Id)).AsGuid().PrimaryKey()
			.WithColumn(nameof(Role.Name)).AsString(60)
			.WithColumn(nameof(Role.NormalizedName)).AsString(60).Indexed()
			.WithColumn(nameof(Role.ConcurrencyStamp)).AsString(128);

		Create.Table(DataConstants.AccountRolesTableName)
			.WithColumn(nameof(IdentityUserRole<Guid>.UserId)).AsGuid().Indexed()
				.ForeignKey(DataConstants.AccountsTableName, nameof(Account.Id)).OnDelete(Rule.Cascade)
			.WithColumn(nameof(IdentityUserRole<Guid>.RoleId)).AsGuid().Indexed()
				.ForeignKey(DataConstants.RolesTableName, nameof(Role.Id)).OnDelete(Rule.Cascade);
	}

	public override void Down()
	{
		Delete.Table(DataConstants.AccountsTableName);
		Delete.Table(DataConstants.AccountClaimsTableName);
		Delete.Table(DataConstants.RolesTableName);
		Delete.Table(DataConstants.AccountRolesTableName);
	}
}