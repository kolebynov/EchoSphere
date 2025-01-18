var builder = DistributedApplication.CreateBuilder(args);
var profileName = builder.Configuration["DOTNET_LAUNCH_PROFILE"] ?? "http";
var postgresUserName = builder.AddParameter("postgresUserName", () => "postgres", secret: false);
var postgresPassword = builder.AddParameter("postgresPassword", () => "postgres", secret: true);

var postgres = builder.AddPostgres("postgres", postgresUserName, postgresPassword, 5433)
	.WithImageTag("latest");

var kafka = builder.AddKafka("kafka")
	.WithImageTag("latest")
	.WithKafkaUI(x => x.WithHostPort(9100).WithImageTag("latest"));

var userMessagesDb = postgres.AddDatabase("UserMessagesDb");
var usersDb = postgres.AddDatabase("UsersDb");
var accountsDb = postgres.AddDatabase("AccountsDb");
var postsDb = postgres.AddDatabase("PostsDb");
var notificationsDb = postgres.AddDatabase("NotificationsDb");

var usersApi = builder.AddProject<Projects.EchoSphere_Users_Api>("UsersApi")
	.WithReference(usersDb)
	.WithReference(kafka)
	.WaitFor(usersDb);

var userMessageApi = builder.AddProject<Projects.EchoSphere_Messages_Api>("MessagesApi")
	.WithReference(userMessagesDb)
	.WithReference(usersApi)
	.WithReference(kafka)
	.WaitFor(userMessagesDb);

var postsApi = builder.AddProject<Projects.EchoSphere_Posts_Api>("PostsApi")
	.WithReference(postsDb)
	.WithReference(kafka)
	.WaitFor(postsDb);

var notificationsApi = builder.AddProject<Projects.EchoSphere_Notifications_Api>("NotificationsApi")
	.WithReference(usersApi)
	.WithReference(notificationsDb)
	.WithReference(kafka)
	.WaitFor(notificationsDb);

var apiService = builder.AddProject<Projects.EchoSphere_ApiGateway>("ApiGateway")
	.WithReference(userMessageApi)
	.WithReference(postsApi)
	.WithReference(notificationsApi)
	.WithReference(usersApi);

var realtimeNotificationsApi = builder.AddProject<Projects.EchoSphere_RealtimeNotifications_Api>("RealtimeNotificationsApi")
	.WithExternalHttpEndpoints()
	.WithReference(kafka)
	.WithReference(userMessageApi)
	.WaitFor(kafka);

var accountsWebApp = builder.AddProject<Projects.EchoSphere_Accounts_WebApp>("AccountsWebApp")
	.WithExternalHttpEndpoints()
	.WithReference(accountsDb)
	.WaitFor(accountsDb);

var accountsEndpoint = accountsWebApp.GetEndpoint(profileName);

var webApp = builder.AddProject<Projects.EchoSphere_Web>("webfrontend")
	.WithExternalHttpEndpoints()
	.WithReference(apiService)
	.WithReference(accountsWebApp)
	.WithReference(realtimeNotificationsApi)
	.WithEnvironment("IdentityUrl", accountsEndpoint);

var webAppEndpoint = webApp.GetEndpoint(profileName);
accountsWebApp.WithEnvironment("WebAppClient", webAppEndpoint);
webApp.WithEnvironment("CallBackUrl", webAppEndpoint);

builder.Build().Run();