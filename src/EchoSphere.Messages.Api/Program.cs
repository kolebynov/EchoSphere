using System.Reflection;
using EchoSphere.Infrastructure.Db.Extensions;
using EchoSphere.Messages.Abstractions;
using EchoSphere.Messages.Abstractions.Models;
using EchoSphere.Messages.Api.Data;
using EchoSphere.Messages.Api.Data.Models;
using EchoSphere.Messages.Api.GrpcServices;
using EchoSphere.Messages.Api.Services;
using EchoSphere.ServiceDefaults;
using EchoSphere.SharedModels.LinqToDb.Extensions;
using EchoSphere.Users.Abstractions.Models;
using LinqToDB.Mapping;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddGrpc();
builder.Services.AddAsyncInitialization();

var mappingSchema = new MappingSchema();
var fluentMappingBuilder = new FluentMappingBuilder(mappingSchema);

fluentMappingBuilder.Entity<ChatParticipantDb>()
	.HasTableName(DataConstants.ChatParticipantsTableName);

fluentMappingBuilder.Entity<ChatMessageDb>()
	.HasTableName(DataConstants.ChatMessagesTableName)
	.HasPrimaryKey(x => x.Id)
	.HasIdentity(x => x.Id);

fluentMappingBuilder.Build();

mappingSchema
	.AddGuidIdValueConverter<UserId>()
	.AddGuidIdValueConverter<ChatId>()
	.AddLongIdValueConverter<MessageId>();

builder.Services.AddLinqToDb<AppDataConnection>(builder.Configuration, "UserMessagesDb", mappingSchema,
	Assembly.GetExecutingAssembly());

builder.Services.AddScoped<IChatService, ChatService>();

var app = builder.Build();

app.MapGrpcService<ChatServiceGrpc>();

await app.InitAndRunAsync();