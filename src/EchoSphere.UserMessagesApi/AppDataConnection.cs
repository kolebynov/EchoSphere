using EchoSphere.UserMessagesApi.Models;
using LinqToDB;
using LinqToDB.Data;

namespace EchoSphere.UserMessagesApi;

internal sealed class AppDataConnection : DataConnection
{
	public ITable<UserMessage> UserMessages => this.GetTable<UserMessage>();

	public AppDataConnection(DataOptions<AppDataConnection> options)
		: base(options.Options)
	{
	}
}