using EchoSphere.Messages.Api.Data.Models;
using FluentMigrator;
using FluentMigrator.Postgres;

namespace EchoSphere.Messages.Api.Data.Migrations;

[Migration(1)]
public sealed class InitialMigration : Migration
{
	public override void Up()
	{
		Create.Table(DataConstants.ChatParticipantsTableName)
			.WithColumn(nameof(ChatParticipantDb.ChatId)).AsGuid().Indexed().NotNullable()
			.WithColumn(nameof(ChatParticipantDb.UserId)).AsGuid().Indexed().NotNullable();

		Create.Table(DataConstants.ChatMessagesTableName)
			.WithColumn(nameof(ChatMessageDb.Id)).AsInt64().PrimaryKey().Identity(PostgresGenerationType.Always)
			.WithColumn(nameof(ChatMessageDb.ChatId)).AsGuid().Indexed().NotNullable()
			.WithColumn(nameof(ChatMessageDb.SenderId)).AsGuid().NotNullable()
			.WithColumn(nameof(ChatMessageDb.Timestamp)).AsDateTimeOffset().Indexed().NotNullable()
			.WithColumn(nameof(ChatMessageDb.Text)).AsString(4000).NotNullable();
	}

	public override void Down()
	{
		Delete.Table(DataConstants.ChatParticipantsTableName);
	}
}