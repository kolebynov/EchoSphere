var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
	.WithImageTag("latest");

var userMessagesDb = postgres.AddDatabase("usermessagesdb");

var userMessageService = builder.AddProject<Projects.EchoSphere_UserMessagesService>("usermessages")
	.WithReference(userMessagesDb);

var apiService = builder.AddProject<Projects.EchoSphere_ApiService>("apiservice")
	.WithReference(userMessageService);

builder.AddProject<Projects.EchoSphere_Web>("webfrontend")
    .WithReference(apiService);

builder.Build().Run();