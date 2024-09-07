using EchoSphere.Accounts.WebApp.Models;
using LinqToDB;
using LinqToDB.Data;

namespace EchoSphere.Accounts.WebApp.Data;

internal sealed class AppDataConnection : DataConnection
{
	public ITable<Account> Accounts => this.GetTable<Account>();

	public AppDataConnection(DataOptions<AppDataConnection> options)
		: base(options.Options)
	{
	}
}