var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
	.WithImageTag("latest");

var userMessagesDb = postgres.AddDatabase("UserMessagesDb");
var usersDb = postgres.AddDatabase("UsersDb");

var userMessageApi = builder.AddProject<Projects.EchoSphere_UserMessagesApi>("UserMessagesApi")
	.WithReference(userMessagesDb);

var usersApi = builder.AddProject<Projects.EchoSphere_UsersApi>("UsersApi")
	.WithReference(usersDb);

var apiService = builder.AddProject<Projects.EchoSphere_ApiService>("apiservice")
	.WithReference(userMessageApi)
	.WithReference(usersApi);

builder.AddProject<Projects.EchoSphere_Web>("webfrontend")
	.WithReference(apiService);

builder.Build().Run();