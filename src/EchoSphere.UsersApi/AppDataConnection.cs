using EchoSphere.UsersApi.Models;
using LinqToDB;
using LinqToDB.Data;

namespace EchoSphere.UsersApi;

internal sealed class AppDataConnection : DataConnection
{
	public ITable<UserProfile> UserProfiles => this.GetTable<UserProfile>();

	public AppDataConnection(DataOptions<AppDataConnection> options)
		: base(options.Options)
	{
	}
}