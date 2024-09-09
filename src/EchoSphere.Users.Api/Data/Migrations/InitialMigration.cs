using EchoSphere.Users.Abstractions.Models;
using EchoSphere.Users.Api.Data.Models;
using FluentMigrator;

namespace EchoSphere.Users.Api.Data.Migrations;

[Migration(1)]
public sealed class InitialMigration : Migration
{
	public override void Up()
	{
		Create.Table(DataConstants.UserProfilesTableName)
			.WithColumn(nameof(UserProfile.Id)).AsGuid().PrimaryKey().NotNullable()
			.WithColumn(nameof(UserProfile.FirstName)).AsString(50).NotNullable()
			.WithColumn(nameof(UserProfile.SecondName)).AsString(50).NotNullable();

		Create.Table(DataConstants.FriendsTableName)
			.WithColumn(nameof(FriendLinkDb.User1Id)).AsGuid().Indexed()
				.ForeignKey(DataConstants.UserProfilesTableName, nameof(UserProfile.Id)).NotNullable()
			.WithColumn(nameof(FriendLinkDb.User2Id)).AsGuid().Indexed()
				.ForeignKey(DataConstants.UserProfilesTableName, nameof(UserProfile.Id)).NotNullable();

		Create.Table(DataConstants.FriendInvitesTableName)
			.WithColumn(nameof(FriendInviteDb.FromUserId)).AsGuid().Indexed()
				.ForeignKey(DataConstants.UserProfilesTableName, nameof(UserProfile.Id)).NotNullable()
			.WithColumn(nameof(FriendInviteDb.ToUserId)).AsGuid().Indexed()
				.ForeignKey(DataConstants.UserProfilesTableName, nameof(UserProfile.Id)).NotNullable();

		Create.Table(DataConstants.FollowersTableName)
			.WithColumn(nameof(FollowerDb.UserId)).AsGuid().Indexed()
				.ForeignKey(DataConstants.UserProfilesTableName, nameof(UserProfile.Id)).NotNullable()
			.WithColumn(nameof(FollowerDb.FollowerUserId)).AsGuid().Indexed()
				.ForeignKey(DataConstants.UserProfilesTableName, nameof(UserProfile.Id)).NotNullable();
	}

	public override void Down()
	{
		Delete.Table(DataConstants.UserProfilesTableName);
		Delete.Table(DataConstants.FriendsTableName);
		Delete.Table(DataConstants.FriendInvitesTableName);
		Delete.Table(DataConstants.FollowersTableName);
	}
}