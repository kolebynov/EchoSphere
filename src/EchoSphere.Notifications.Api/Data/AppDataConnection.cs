using LinqToDB;
using LinqToDB.Data;

namespace EchoSphere.Notifications.Api.Data;

internal sealed class AppDataConnection : DataConnection
{
	public AppDataConnection(DataOptions<AppDataConnection> options)
		: base(options.Options)
	{
	}
}