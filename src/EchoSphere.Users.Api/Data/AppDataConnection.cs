using LinqToDB;
using LinqToDB.Data;

namespace EchoSphere.Users.Api.Data;

internal sealed class AppDataConnection : DataConnection
{
	public AppDataConnection(DataOptions<AppDataConnection> options)
		: base(options.Options)
	{
	}
}