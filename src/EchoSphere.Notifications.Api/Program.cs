using System.Reflection;
using EchoSphere.Domain.Abstractions.Models;
using EchoSphere.Domain.AspNetCore.Extensions;
using EchoSphere.Domain.LinqToDb.Extensions;
using EchoSphere.Infrastructure.Db.Extensions;
using EchoSphere.Infrastructure.IntegrationEvents.Extensions;
using EchoSphere.Notifications.Abstractions;
using EchoSphere.Notifications.Abstractions.Models;
using EchoSphere.Notifications.Api.Abstractions;
using EchoSphere.Notifications.Api.Data;
using EchoSphere.Notifications.Api.Data.Models;
using EchoSphere.Notifications.Api.EventToNotificationsConverters;
using EchoSphere.Notifications.Api.Extensions;
using EchoSphere.Notifications.Api.GrpcServices;
using EchoSphere.Notifications.Api.Services;
using EchoSphere.Posts.Abstractions.IntegrationEvents;
using EchoSphere.ServiceDefaults;
using EchoSphere.Users.Client.Extensions;
using LinqToDB.Mapping;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults().AddIntegrationEvents();

builder.Services.AddGrpc();
builder.Services.AddAsyncInitialization();
builder.Services.AddDomainServices();

builder.Services.AddLinqToDb<AppDataConnection>(dbSettings =>
{
	dbSettings.ConnectionStringName = "NotificationsDb";
	dbSettings.MigrationAssemblies = [Assembly.GetExecutingAssembly()];

	var mappingSchema = dbSettings.MappingSchema;
	var fluentMappingBuilder = new FluentMappingBuilder(mappingSchema);

	fluentMappingBuilder.Entity<NotificationDb>()
		.HasTableName(DataConstants.NotificationsTableName)
		.HasPrimaryKey(x => x.Id)
		.HasIdentity(x => x.Id);

	fluentMappingBuilder.Build();

	mappingSchema
		.AddIdValueConverter<Guid, UserId>()
		.AddIdValueConverter<long, NotificationId>();
});

builder.Services.AddUsersGrpcClient(new Uri("https://UsersApi"));

builder.Services.AddScoped<NotificationService>();
builder.Services.AddTransient<INotificationService>(sp => sp.GetRequiredService<NotificationService>());
builder.Services.AddTransient<INotificationStorage>(sp => sp.GetRequiredService<NotificationService>());

builder.Services.AddEventToNotificationConverter<PostPublished, PostPublishedConverter>();
builder.Services.AddEventToNotificationConverter<PostLiked, PostLikedConverter>();
builder.Services.AddEventToNotificationConverter<PostCommentAdded, PostCommentAddedConverter>();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapGrpcService<NotificationServiceGrpc>();

await app.InitAndRunAsync();