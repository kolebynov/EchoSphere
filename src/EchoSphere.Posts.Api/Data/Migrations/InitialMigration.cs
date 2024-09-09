using EchoSphere.Posts.Api.Data.Models;
using FluentMigrator;

namespace EchoSphere.Posts.Api.Data.Migrations;

[Migration(1)]
public sealed class InitialMigration : Migration
{
	public override void Up()
	{
		Create.Table(DataConstants.PostsTableName)
			.WithColumn(nameof(PostDb.Id)).AsGuid().PrimaryKey().NotNullable()
			.WithColumn(nameof(PostDb.UserId)).AsGuid().Indexed().NotNullable()
			.WithColumn(nameof(PostDb.Title)).AsString(120).NotNullable()
			.WithColumn(nameof(PostDb.Body)).AsString(4000).NotNullable();
	}

	public override void Down()
	{
		Delete.Table(DataConstants.PostsTableName);
	}
}