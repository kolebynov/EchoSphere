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
			.WithColumn(nameof(PostDb.PostedOn)).AsDateTime().Indexed().NotNullable()
			.WithColumn(nameof(PostDb.AuthorId)).AsGuid().Indexed().NotNullable()
			.WithColumn(nameof(PostDb.Body)).AsString(4000).NotNullable();

		Create.Table(DataConstants.PostLikesTableName)
			.WithColumn(nameof(PostLikeDb.UserId)).AsGuid().Indexed().NotNullable()
			.WithColumn(nameof(PostLikeDb.PostId)).AsGuid().Indexed().NotNullable();

		Create.Table(DataConstants.PostCommentsTableName)
			.WithColumn(nameof(PostCommentDb.Id)).AsGuid().PrimaryKey().NotNullable()
			.WithColumn(nameof(PostCommentDb.UserId)).AsGuid().Indexed().NotNullable()
			.WithColumn(nameof(PostCommentDb.Text)).AsString(1024).NotNullable()
			.WithColumn(nameof(PostCommentDb.PostId)).AsGuid().Indexed().NotNullable();
	}

	public override void Down()
	{
		Delete.Table(DataConstants.PostsTableName);
		Delete.Table(DataConstants.PostLikesTableName);
		Delete.Table(DataConstants.PostCommentsTableName);
	}
}