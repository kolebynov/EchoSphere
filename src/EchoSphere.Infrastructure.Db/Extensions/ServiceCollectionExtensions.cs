using System.Globalization;
using EchoSphere.Infrastructure.Db.Settings;
using EchoSphere.Infrastructure.Hosting.Extensions;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Initialization;
using LinqToDB;
using LinqToDB.AspNet;
using LinqToDB.AspNet.Logging;
using LinqToDB.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Npgsql;

namespace EchoSphere.Infrastructure.Db.Extensions;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddLinqToDb<TContext>(
		this IServiceCollection services, Action<DbSettings>? configureAction = null)
		where TContext : DataConnection
	{
		services.AddLinqToDBContext<TContext>(
			(provider, options) =>
			{
				var dbSettings = provider.GetRequiredService<IOptions<DbSettings>>().Value;
				var connectionString = provider.GetRequiredService<IConfiguration>().GetConnectionString(dbSettings.ConnectionStringName)!;
				return options
					.UsePostgreSQL(connectionString)
					.UseDefaultLogging(provider)
					.UseMappingSchema(dbSettings.MappingSchema);
			});

		services
			.AddFluentMigratorCore()
			.ConfigureRunner(rb =>
			{
				rb
					.AddPostgres()
					.WithGlobalConnectionString(sp =>
					{
						var dbSettings = sp.GetRequiredService<IOptions<DbSettings>>().Value;
						return sp.GetRequiredService<IConfiguration>().GetConnectionString(dbSettings.ConnectionStringName)!;
					});
			});

		services.AddScopedAsyncInitializer(async (sp, ct) =>
		{
			await EnsureDatabase(sp.GetRequiredService<DataOptions<TContext>>(), ct);
			sp.GetRequiredService<IMigrationRunner>().MigrateUp();
		});

		var dbSettingsBuilder = services.AddOptions<DbSettings>().PostConfigure(settings =>
		{
			settings.MappingSchema.SetConvertExpression<DateTime, DateTimeOffset>(x => x.UnspecifiedAsUtc());
		});

		if (configureAction != null)
		{
			dbSettingsBuilder.Configure(configureAction);
		}

		services.AddTransient<IDataContext>(sp => sp.GetRequiredService<TContext>());
		services.AddTransient<DataConnection>(sp => sp.GetRequiredService<TContext>());

		services.AddSingleton<IMigrationSourceItem>(sp =>
		{
			var dbSettings = sp.GetRequiredService<IOptions<DbSettings>>().Value;
			return new AssemblyMigrationSourceItem(dbSettings.MigrationAssemblies);
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

	private static DateTime UnspecifiedAsUtc(this DateTime dateTime) =>
		dateTime.Kind == DateTimeKind.Unspecified
			? DateTime.SpecifyKind(dateTime, DateTimeKind.Utc)
			: dateTime;
}