var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
	.WithImageTag("latest");

var userMessagesDb = postgres.AddDatabase("UserMessagesDb");
var usersDb = postgres.AddDatabase("UsersDb");
var accountsDb = postgres.AddDatabase("AccountsDb");

var userMessageApi = builder.AddProject<Projects.EchoSphere_Messages_Api>("MessagesApi")
	.WithReference(userMessagesDb);

var usersApi = builder.AddProject<Projects.EchoSphere_Users_Api>("UsersApi")
	.WithReference(usersDb);

var apiService = builder.AddProject<Projects.EchoSphere_ApiGateway>("ApiGateway")
	.WithReference(userMessageApi)
	.WithReference(usersApi);

var accountsWebApp = builder.AddProject<Projects.EchoSphere_Accounts_WebApp>("AccountsWebApp")
	.WithExternalHttpEndpoints()
	.WithReference(accountsDb);

var accountsEndpoint = accountsWebApp.GetEndpoint("http");

var webApp = builder.AddProject<Projects.EchoSphere_Web>("webfrontend")
	.WithExternalHttpEndpoints()
	.WithReference(apiService)
	.WithEnvironment("IdentityUrl", accountsEndpoint)
	.WithReference(accountsWebApp);

var webAppEndpoint = webApp.GetEndpoint("http");
accountsWebApp.WithEnvironment("WebAppClient", webAppEndpoint);
webApp.WithEnvironment("CallBackUrl", webAppEndpoint);

builder.Build().Run();