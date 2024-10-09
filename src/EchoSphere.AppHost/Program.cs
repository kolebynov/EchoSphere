var builder = DistributedApplication.CreateBuilder(args);
var profileName = builder.Configuration["DOTNET_LAUNCH_PROFILE"] ?? "http";

var postgres = builder.AddPostgres("postgres")
	.WithImageTag("latest");

var userMessagesDb = postgres.AddDatabase("UserMessagesDb");
var usersDb = postgres.AddDatabase("UsersDb");
var accountsDb = postgres.AddDatabase("AccountsDb");
var postsDb = postgres.AddDatabase("PostsDb");

var usersApi = builder.AddProject<Projects.EchoSphere_Users_Api>("UsersApi")
	.WithReference(usersDb);

var userMessageApi = builder.AddProject<Projects.EchoSphere_Messages_Api>("MessagesApi")
	.WithReference(userMessagesDb)
	.WithReference(usersApi);

var postsApi = builder.AddProject<Projects.EchoSphere_Posts_Api>("PostsApi")
	.WithReference(postsDb);

var apiService = builder.AddProject<Projects.EchoSphere_ApiGateway>("ApiGateway")
	.WithReference(userMessageApi)
	.WithReference(postsApi)
	.WithReference(usersApi);

var accountsWebApp = builder.AddProject<Projects.EchoSphere_Accounts_WebApp>("AccountsWebApp")
	.WithExternalHttpEndpoints()
	.WithReference(accountsDb);

var accountsEndpoint = accountsWebApp.GetEndpoint(profileName);

var webApp = builder.AddProject<Projects.EchoSphere_Web>("webfrontend")
	.WithExternalHttpEndpoints()
	.WithReference(apiService)
	.WithEnvironment("IdentityUrl", accountsEndpoint)
	.WithReference(accountsWebApp);

var webAppEndpoint = webApp.GetEndpoint(profileName);
accountsWebApp.WithEnvironment("WebAppClient", webAppEndpoint);
webApp.WithEnvironment("CallBackUrl", webAppEndpoint);

builder.Build().Run();