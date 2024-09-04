using EchoSphere.UsersApi.Models;
using FluentMigrator;

namespace EchoSphere.UsersApi.Migrations;

[Migration(1)]
public sealed class InitialMigration : Migration
{
	public override void Up()
	{
		Create.Table("UserProfiles")
			.WithColumn(nameof(UserProfile.Id)).AsGuid().PrimaryKey().NotNullable()
			.WithColumn(nameof(UserProfile.UserName)).AsString(60)
			.WithColumn(nameof(UserProfile.NormalizedUserName)).AsString(60).Indexed()
			.WithColumn(nameof(UserProfile.ConcurrencyStamp)).AsString()
			.WithColumn(nameof(UserProfile.PasswordHash)).AsString()
			.WithColumn(nameof(UserProfile.Email)).AsString(60)
			.WithColumn(nameof(UserProfile.EmailConfirmed)).AsBoolean()
			.WithColumn(nameof(UserProfile.LockoutEnabled)).AsBoolean()
			.WithColumn(nameof(UserProfile.LockoutEnd)).AsDateTimeOffset().Nullable()
			.WithColumn(nameof(UserProfile.NormalizedEmail)).AsString(60).Indexed()
			.WithColumn(nameof(UserProfile.PhoneNumber)).AsString(20).Nullable()
			.WithColumn(nameof(UserProfile.SecurityStamp)).AsString()
			.WithColumn(nameof(UserProfile.AccessFailedCount)).AsInt32()
			.WithColumn(nameof(UserProfile.PhoneNumberConfirmed)).AsBoolean()
			.WithColumn(nameof(UserProfile.TwoFactorEnabled)).AsBoolean();
	}

	public override void Down()
	{
		Delete.Table("UserProfiles");
	}
}