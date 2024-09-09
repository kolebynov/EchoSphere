using System.Globalization;
using System.Reflection;
using EchoSphere.Infrastructure.Hosting.Extensions;
using FluentMigrator.Runner;
using LinqToDB;
using LinqToDB.AspNet;
using LinqToDB.AspNet.Logging;
using LinqToDB.Data;
using LinqToDB.Mapping;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace EchoSphere.Infrastructure.Db.Extensions;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddLinqToDb<TContext>(
		this IServiceCollection services, IConfiguration configuration, string name, MappingSchema? mappingSchema,
		Assembly migrationsAssembly)
		where TContext : DataConnection
	{
		var connectionString = configuration.GetConnectionString(name)!;
		services.AddLinqToDBContext<TContext>(
			(provider, options) =>
			{
				options = options
					.UsePostgreSQL(connectionString)
					.UseDefaultLogging(provider);
				return mappingSchema != null ? options.UseMappingSchema(mappingSchema) : options;
			});

		services.AddTransient<IDataContext>(sp => sp.GetRequiredService<TContext>());
		services.AddTransient<DataConnection>(sp => sp.GetRequiredService<TContext>());

		services
			.AddFluentMigratorCore()
			.ConfigureRunner(rb => rb
				.AddPostgres()
				.WithGlobalConnectionString(connectionString)
				.ScanIn(migrationsAssembly).For.Migrations());

		services.AddScopedAsyncInitializer(async (sp, ct) =>
		{
			await EnsureDatabase(sp.GetRequiredService<DataOptions<TContext>>(), ct);
			sp.GetRequiredService<IMigrationRunner>().MigrateUp();
		});

		return services;
	}

	private static async ValueTask EnsureDatabase<TContext>(
		DataOptions<TContext> dataOptions, CancellationToken cancellationToken)
		where TContext : DataConnection
	{
		const string databasesQuery = "select datname from postgres.pg_catalog.pg_database where datname = @name";
		const string createDatabaseQuery = "CREATE DATABASE \"{0}\"";

		var connectionStringBuilder =
			new NpgsqlConnectionStringBuilder(dataOptions.Options.ConnectionOptions.ConnectionString!);
		var databaseName = connectionStringBuilder.Database;
		connectionStringBuilder.Database = "postgres";
		var connectionString = connectionStringBuilder.ConnectionString;
		using var dataConnection = new DataConnection(dataOptions.Options.UseConnectionString(connectionString));

		if ((await dataConnection.QueryToArrayAsync<object>(databasesQuery, new DataParameter("name", databaseName), cancellationToken)).Length > 0)
		{
			return;
		}

		var command = string.Format(CultureInfo.InvariantCulture, createDatabaseQuery, databaseName);
		await dataConnection.ExecuteAsync(command, cancellationToken);
	}
}