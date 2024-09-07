using System.Reflection;
using EchoSphere.Infrastructure.Db.Extensions;
using EchoSphere.Infrastructure.Hosting.Extensions;
using EchoSphere.Messages.Abstractions;
using EchoSphere.Messages.Abstractions.Models;
using EchoSphere.Messages.Api.Data;
using EchoSphere.Messages.Api.Data.Models;
using EchoSphere.Messages.Api.Grpc;
using EchoSphere.Messages.Api.Services;
using EchoSphere.ServiceDefaults;
using EchoSphere.Users.Abstractions.Models;
using LinqToDB.Mapping;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddGrpc();
builder.Services.AddAsyncInitialization();

var mappingSchema = new MappingSchema();
var fluentMappingBuilder = new FluentMappingBuilder(mappingSchema);

fluentMappingBuilder.Entity<ChatParticipantDb>()
	.HasTableName(DataConstants.ChatParticipantsTableName)
	.Property(x => x.ChatId).HasConversionFunc(x => x.Value, x => new ChatId(x))
	.Property(x => x.UserId).HasConversionFunc(x => x.Value, x => new UserId(x));

fluentMappingBuilder.Entity<ChatMessageDb>()
	.HasTableName(DataConstants.ChatMessagesTableName)
	.HasPrimaryKey(x => x.Id)
	.HasIdentity(x => x.Id)
	.Property(x => x.ChatId).HasConversionFunc(x => x.Value, x => new ChatId(x))
	.Property(x => x.Id).HasConversionFunc(x => x.Value, x => new MessageId(x))
	.Property(x => x.SenderId).HasConversionFunc(x => x.Value, x => new UserId(x));

fluentMappingBuilder.Build();

builder.Services.AddLinqToDb<AppDataConnection>(builder.Configuration, "UserMessagesDb", mappingSchema,
	Assembly.GetExecutingAssembly());

builder.Services.AddScoped<IChatService, ChatService>();

var app = builder.Build();

app.MapGrpcService<ChatServiceGrpcImpl>();

await app.InitAndRunAsync();