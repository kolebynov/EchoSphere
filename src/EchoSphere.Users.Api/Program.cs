using System.Reflection;
using EchoSphere.Infrastructure.Db.Extensions;
using EchoSphere.Infrastructure.Hosting.Extensions;
using EchoSphere.ServiceDefaults;
using EchoSphere.SharedModels.LinqToDb.Extensions;
using EchoSphere.Users.Abstractions;
using EchoSphere.Users.Abstractions.Models;
using EchoSphere.Users.Api.Data;
using EchoSphere.Users.Api.Data.Models;
using EchoSphere.Users.Api.GrpcServices;
using EchoSphere.Users.Api.Services;
using LinqToDB.Data;
using LinqToDB.Mapping;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddGrpc();
builder.Services.AddAsyncInitialization();

var mappingSchema = new MappingSchema();

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
	.AddGuidIdValueConverter<FriendInvitationId>()
	.AddGuidIdValueConverter<UserId>();

builder.Services.AddLinqToDb<AppDataConnection>(builder.Configuration, "UsersDb", mappingSchema,
	Assembly.GetExecutingAssembly());

builder.Services.AddScoped<IFriendService, FriendService>();
builder.Services.AddScoped<IUserProfileService, UserProfileService>();
builder.Services.AddScoped<IFollowService, FollowService>();

// TODO: Remove later
builder.Services.AddScopedAsyncInitializer(async (sp, ct) =>
{
	var dataContext = sp.GetRequiredService<DataConnection>();
	await dataContext.BulkCopyAsync(
		[
			new UserProfile
			{
				Id = new UserId(new Guid("cd2ecf42-deb8-4381-9567-3754cea3465a")),
				FirstName = "Test1",
				SecondName = "Test1",
			},
			new UserProfile
			{
				Id = new UserId(new Guid("e6da2482-630f-4755-9b13-f4b4733c0ad5")),
				FirstName = "Test2",
				SecondName = "Test2",
			},
		],
		ct);
});

var app = builder.Build();

app.MapGrpcService<UserProfileServiceGrpc>();
app.MapGrpcService<FriendServiceGrpc>();
app.MapGrpcService<FollowServiceGrpc>();

await app.InitAndRunAsync();