using System.Reflection;
using EchoSphere.Infrastructure.Db.Extensions;
using EchoSphere.UserMessagesApi;
using EchoSphere.UserMessagesApi.Models;
using EchoSphere.UserMessagesApi.Services;
using LinqToDB.Mapping;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddGrpc();

var mappingSchema = new MappingSchema();
var fluentMappingBuilder = new FluentMappingBuilder(mappingSchema);

fluentMappingBuilder.Entity<UserMessage>()
	.HasTableName("UserMessages")
	.HasIdentity(x => x.Id)
	.HasPrimaryKey(x => x.Id);

fluentMappingBuilder.Build();

builder.Services.AddPostgresDb<AppDataConnection>(builder.Configuration, "UserMessagesDb", mappingSchema,
	Assembly.GetExecutingAssembly());

var app = builder.Build();

app.MapGrpcService<MessagesService>();

await app.InitAndRunAsync();