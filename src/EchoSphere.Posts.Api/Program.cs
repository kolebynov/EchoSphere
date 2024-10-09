using System.Reflection;
using EchoSphere.Domain.Abstractions.Models;
using EchoSphere.Domain.AspNetCore.Extensions;
using EchoSphere.Domain.LinqToDb.Extensions;
using EchoSphere.Infrastructure.Db.Extensions;
using EchoSphere.Posts.Abstractions;
using EchoSphere.Posts.Api.Data;
using EchoSphere.Posts.Api.Data.Models;
using EchoSphere.Posts.Api.GrpcServices;
using EchoSphere.Posts.Api.Services;
using EchoSphere.ServiceDefaults;
using LinqToDB.Mapping;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

builder.Services.AddGrpc();
builder.Services.AddAsyncInitialization();

builder.Services.AddLinqToDb<AppDataConnection>(dbSettings =>
{
	dbSettings.ConnectionStringName = "PostsDb";
	dbSettings.MigrationAssemblies = [Assembly.GetExecutingAssembly()];

	var mappingSchema = dbSettings.MappingSchema;
	var fluentMappingBuilder = new FluentMappingBuilder(mappingSchema);

	fluentMappingBuilder.Entity<PostDb>()
		.HasTableName(DataConstants.PostsTableName)
		.HasPrimaryKey(x => x.Id);

	fluentMappingBuilder.Build();

	mappingSchema
		.AddIdValueConverter<Guid, PostId>()
		.AddIdValueConverter<Guid, UserId>();
});

builder.Services.AddDomainServices();

builder.Services.AddScoped<IPostService, PostService>();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapGrpcService<PostServiceGrpc>();

await app.InitAndRunAsync();