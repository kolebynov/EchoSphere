using System.Reflection;
using EchoSphere.Infrastructure.Db.Extensions;
using EchoSphere.UsersApi;
using EchoSphere.UsersApi.Extensions;
using EchoSphere.UsersApi.Models;
using EchoSphere.UsersApi.Services;
using LinqToDB.Mapping;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddGrpc();

builder.Services.AddIdentity<UserProfile, UserRole>()
	.AddLinqToDbStores();

var mappingSchema = new MappingSchema();
var fluentMappingBuilder = new FluentMappingBuilder(mappingSchema);

fluentMappingBuilder.Entity<UserProfile>()
	.HasTableName("UserProfiles")
	.HasPrimaryKey(x => x.Id);

fluentMappingBuilder.Build();

builder.Services.AddPostgresDb<AppDataConnection>(builder.Configuration, "UsersDb", mappingSchema,
	Assembly.GetExecutingAssembly());

var app = builder.Build();

app.MapGrpcService<UsersService>();

await app.InitAndRunAsync();