using EchoSphere.UserMessagesApi.Models;
using FluentMigrator;

namespace EchoSphere.UserMessagesApi.Migrations;

[Migration(1)]
public sealed class InitialMigration : Migration
{
	public override void Up()
	{
		Create.Table("UserMessages")
			.WithColumn(nameof(UserMessage.Id)).AsInt64().PrimaryKey().Identity().NotNullable()
			.WithColumn(nameof(UserMessage.FromUserId)).AsGuid().Indexed().NotNullable()
			.WithColumn(nameof(UserMessage.ToUserId)).AsGuid().Indexed().NotNullable()
			.WithColumn(nameof(UserMessage.Text)).AsString(1024).NotNullable();
	}

	public override void Down()
	{
		Delete.Table("UserMessages");
	}
}