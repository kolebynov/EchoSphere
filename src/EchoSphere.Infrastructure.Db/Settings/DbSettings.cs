using System.Reflection;
using LinqToDB.Mapping;

namespace EchoSphere.Infrastructure.Db.Settings;

public sealed class DbSettings
{
	public required string ConnectionStringName { get; set; }

	public required IReadOnlyCollection<Assembly> MigrationAssemblies { get; set; }

	public MappingSchema MappingSchema { get; } = new();
}