using System.Reflection;
using EchoSphere.Infrastructure.Db.Extensions;
using EchoSphere.Posts.Abstractions;
using EchoSphere.Posts.Abstractions.Models;
using EchoSphere.Posts.Api.Data;
using EchoSphere.Posts.Api.Data.Models;
using EchoSphere.Posts.Api.GrpcServices;
using EchoSphere.Posts.Api.Services;
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

fluentMappingBuilder.Entity<PostDb>()
	.HasTableName(DataConstants.PostsTableName)
	.HasPrimaryKey(x => x.Id);

fluentMappingBuilder.Build();

mappingSchema
	.AddGuidIdValueConverter<PostId>()
	.AddGuidIdValueConverter<UserId>();

builder.Services.AddLinqToDb<AppDataConnection>(builder.Configuration, "PostsDb", mappingSchema,
	Assembly.GetExecutingAssembly());

builder.Services.AddScoped<IPostService, PostService>();

var app = builder.Build();

app.MapGrpcService<PostServiceGrpc>();

await app.InitAndRunAsync();