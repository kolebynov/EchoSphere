using System.Data.Common;
using System.Globalization;
using EchoSphere.UserMessagesService;
using EchoSphere.UserMessagesService.Models;
using EchoSphere.UserMessagesService.Services;
using FluentMigrator.Runner;
using LinqToDB;
using LinqToDB.AspNet;
using LinqToDB.AspNet.Logging;
using LinqToDB.Data;
using LinqToDB.Mapping;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddGrpc();

var mappingSchema = new MappingSchema();
var fluentMappingBuilder = new FluentMappingBuilder(mappingSchema);

fluentMappingBuilder.Entity<UserMessage>()
	.HasTableName("UserMessages");

fluentMappingBuilder.Build();

builder.Services.AddNpgsqlDataSource("usermessagesdb");

var connectionString = builder.Configuration.GetConnectionString("usermessagesdb")!;
builder.Services.AddLinqToDBContext<AppDataConnection>(
	(provider, options)
		=> options
			.UsePostgreSQL(connectionString)
			.UseDefaultLogging(provider)
			.UseMappingSchema(mappingSchema));

builder.Services
	.AddFluentMigratorCore()
	.ConfigureRunner(rb => rb
		.AddPostgres()
		.WithGlobalConnectionString(connectionString)
		.ScanIn(typeof(Program).Assembly).For.Migrations());

var app = builder.Build();

using (var serviceScope = app.Services.CreateScope())
{
	EnsureDatabase(serviceScope.ServiceProvider.GetRequiredService<DataOptions<AppDataConnection>>());
	serviceScope.ServiceProvider.GetRequiredService<IMigrationRunner>().MigrateUp();
}

app.MapGrpcService<MessageService>();
app.MapGet("/",
	() =>
		"Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();

static void EnsureDatabase(DataOptions<AppDataConnection> dataOptions)
{
	const string databasesQuery = "select datname from postgres.pg_catalog.pg_database where datname = @name";
	const string createDatabaseQuery = "CREATE DATABASE \"{0}\"";

	var connectionStringBuilder =
		new NpgsqlConnectionStringBuilder(dataOptions.Options.ConnectionOptions.ConnectionString!);
	var databaseName = connectionStringBuilder.Database;
	connectionStringBuilder.Database = "postgres";
	var connectionString = connectionStringBuilder.ConnectionString;
	using var dataConnection = new DataConnection(dataOptions.Options.UseConnectionString(connectionString));

	if (dataConnection.Query<object>(databasesQuery, new DataParameter("name", databaseName)).Any())
	{
		return;
	}

	var command = string.Format(CultureInfo.InvariantCulture, createDatabaseQuery, databaseName);
	dataConnection.Execute(command);
}