using EchoSphere.Notifications.Api.Data.Models;
using FluentMigrator;

namespace EchoSphere.Notifications.Api.Data.Migrations;

[Migration(1)]
public sealed class InitialMigration : Migration
{
	public override void Up()
	{
		Create.Table(DataConstants.NotificationsTableName)
			.WithColumn(nameof(NotificationDb.Id)).AsInt64().PrimaryKey().Identity()
			.WithColumn(nameof(NotificationDb.Text)).AsString().NotNullable()
			.WithColumn(nameof(NotificationDb.IsRead)).AsBoolean().NotNullable()
			.WithColumn(nameof(NotificationDb.UserId)).AsGuid().Indexed().NotNullable();
	}

	public override void Down()
	{
		Delete.Table(DataConstants.NotificationsTableName);
	}
}