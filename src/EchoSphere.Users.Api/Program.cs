using System.Reflection;
using EchoSphere.Domain.Abstractions;
using EchoSphere.Domain.Abstractions.Models;
using EchoSphere.Domain.AspNetCore.Extensions;
using EchoSphere.Domain.LinqToDb.Extensions;
using EchoSphere.Infrastructure.Db.Extensions;
using EchoSphere.Infrastructure.Hosting.Extensions;
using EchoSphere.ServiceDefaults;
using EchoSphere.Users.Abstractions;
using EchoSphere.Users.Abstractions.Models;
using EchoSphere.Users.Api.Data;
using EchoSphere.Users.Api.Data.Models;
using EchoSphere.Users.Api.GrpcServices;
using EchoSphere.Users.Api.Services;
using LanguageExt.UnsafeValueAccess;
using LinqToDB;
using LinqToDB.Data;
using LinqToDB.Mapping;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddGrpc();
builder.Services.AddAsyncInitialization();

builder.Services.AddLinqToDb<AppDataConnection>(dbSettings =>
{
	dbSettings.ConnectionStringName = "UsersDb";
	dbSettings.MigrationAssemblies = [Assembly.GetExecutingAssembly()];

	var mappingSchema = dbSettings.MappingSchema;
	var fluentMappingBuilder = new FluentMappingBuilder(mappingSchema);

	fluentMappingBuilder.Entity<UserProfile>()
		.HasTableName(DataConstants.UserProfilesTableName)
		.HasPrimaryKey(x => x.Id);

	fluentMappingBuilder.Entity<FriendInvitationDb>()
		.HasTableName(DataConstants.FriendInvitesTableName)
		.HasPrimaryKey(x => x.Id);

	fluentMappingBuilder.Entity<FriendLinkDb>()
		.HasTableName(DataConstants.FriendsTableName);

	fluentMappingBuilder.Entity<FollowerDb>()
		.HasTableName(DataConstants.FollowersTableName);

	fluentMappingBuilder.Build();

	mappingSchema
		.AddIdValueConverter<Guid, FriendInvitationId>()
		.AddIdValueConverter<Guid, UserId>();
});

builder.Services.AddDomainServices();

builder.Services.AddScoped<IFriendService, FriendService>();
builder.Services.AddScoped<IUserProfileService, UserProfileService>();
builder.Services.AddScoped<IFollowService, FollowService>();

// TODO: Remove later
builder.Services.AddScopedAsyncInitializer(async (sp, ct) =>
{
	var dataContext = sp.GetRequiredService<DataConnection>();
	var test1Id = new UserId(new Guid("cd2ecf42-deb8-4381-9567-3754cea3465a"));
	var test2Id = new UserId(new Guid("e6da2482-630f-4755-9b13-f4b4733c0ad5"));
	if (!await dataContext.GetTable<UserProfile>().AnyAsync(ct))
	{
		await dataContext.BulkCopyAsync(
			[
				new UserProfile
				{
					Id = test1Id,
					FirstName = "Test1",
					SecondName = "Test1",
				},
				new UserProfile
				{
					Id = test2Id,
					FirstName = "Test2",
					SecondName = "Test2",
				},
			],
			ct);
	}

	var currentUserAccessor = new StubCurrentUserAccessor { CurrentUserId = test1Id };
	var friendService = new FriendService(dataContext, sp.GetRequiredService<IFollowService>(),
		sp.GetRequiredService<IUserProfileService>(), currentUserAccessor);
	if ((await friendService.SendFriendInvite(test2Id, CancellationToken.None)).IsRight)
	{
		currentUserAccessor.CurrentUserId = test2Id;
		var inviteId = (await friendService.GetCurrentUserFriendInvites(CancellationToken.None)).ValueUnsafe()[0].Id;
		await friendService.AcceptFriendInvite(inviteId, CancellationToken.None);
	}
});

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapGrpcService<UserProfileServiceGrpc>();
app.MapGrpcService<FriendServiceGrpc>();
app.MapGrpcService<FollowServiceGrpc>();

await app.InitAndRunAsync();

internal sealed class StubCurrentUserAccessor : ICurrentUserAccessor
{
	public UserId CurrentUserId { get; set; }
}