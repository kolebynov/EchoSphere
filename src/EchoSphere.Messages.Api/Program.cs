using System.Reflection;
using EchoSphere.Domain.Abstractions.Models;
using EchoSphere.Domain.AspNetCore.Extensions;
using EchoSphere.Domain.LinqToDb.Extensions;
using EchoSphere.Infrastructure.Db.Extensions;
using EchoSphere.Infrastructure.IntegrationEvents.Extensions;
using EchoSphere.Messages.Abstractions;
using EchoSphere.Messages.Api.Data;
using EchoSphere.Messages.Api.Data.Models;
using EchoSphere.Messages.Api.GrpcServices;
using EchoSphere.Messages.Api.Services;
using EchoSphere.ServiceDefaults;
using EchoSphere.Users.Client.Extensions;
using LinqToDB.Mapping;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddIntegrationEvents();

builder.Services.AddGrpc();
builder.Services.AddAsyncInitialization();

builder.Services.AddLinqToDb<AppDataConnection>(dbSettings =>
{
	dbSettings.ConnectionStringName = "UserMessagesDb";
	dbSettings.MigrationAssemblies = [Assembly.GetExecutingAssembly()];

	var mappingSchema = dbSettings.MappingSchema;
	var fluentMappingBuilder = new FluentMappingBuilder(mappingSchema);

	fluentMappingBuilder.Entity<ChatParticipantDb>()
		.HasTableName(DataConstants.ChatParticipantsTableName);

	fluentMappingBuilder.Entity<ChatMessageDb>()
		.HasTableName(DataConstants.ChatMessagesTableName)
		.HasPrimaryKey(x => x.Id)
		.HasIdentity(x => x.Id);

	fluentMappingBuilder.Build();

	mappingSchema
		.AddIdValueConverter<Guid, UserId>()
		.AddIdValueConverter<Guid, ChatId>()
		.AddIdValueConverter<long, MessageId>();
});

builder.Services.AddUsersGrpcClient(new Uri("https://UsersApi"));
builder.Services.AddDomainServices();

builder.Services.AddScoped<IChatService, ChatService>();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapGrpcService<ChatServiceGrpc>();

await app.InitAndRunAsync();