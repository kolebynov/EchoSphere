using EchoSphere.Infrastructure.IntegrationEvents.Data.Models;
using FluentMigrator;
using FluentMigrator.Postgres;

namespace EchoSphere.Infrastructure.IntegrationEvents.Data.Migrations;

[Migration(0, "Integration events migration")]
public sealed class InitialMigration : Migration
{
	public override void Up()
	{
		Create.Table(DataConstants.EventsTableName)
			.WithColumn(nameof(IntegrationEventDb.Id)).AsInt64().PrimaryKey().Identity()
			.WithColumn(nameof(IntegrationEventDb.TypeName)).AsString().NotNullable()
			.WithColumn(nameof(IntegrationEventDb.EventData)).AsBinary().NotNullable()
			.WithColumn(nameof(IntegrationEventDb.State)).AsInt32().NotNullable();

		Create.Index($"IX_{DataConstants.EventsTableName}_{nameof(IntegrationEventDb.State)}")
			.OnTable(DataConstants.EventsTableName)
			.OnColumn(nameof(IntegrationEventDb.State)).Ascending()
			.WithOptions()
			.Filter($"\"{nameof(IntegrationEventDb.State)}\" in ({(int)IntegrationEventState.Initial}, {(int)IntegrationEventState.Processing})")
			.UsingHash();
	}

	public override void Down()
	{
		Delete.Table(DataConstants.EventsTableName);
	}
}