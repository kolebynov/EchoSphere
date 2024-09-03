using EchoSphere.UserMessagesService.Models;
using LinqToDB;
using LinqToDB.Data;

namespace EchoSphere.UserMessagesService;

internal sealed class AppDataConnection : DataConnection
{
	public ITable<UserMessage> UserMessages => this.GetTable<UserMessage>();

	public AppDataConnection(DataOptions<AppDataConnection> options)
		: base(options.Options)
	{
	}
}