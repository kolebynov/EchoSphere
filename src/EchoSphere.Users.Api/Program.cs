using EchoSphere.ServiceDefaults;
using EchoSphere.Users.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddGrpc();
builder.Services.AddAsyncInitialization();

// builder.Services.AddLinqToDb<AppDataConnection>(builder.Configuration, "UsersDb", null,
// 	Assembly.GetExecutingAssembly());

var app = builder.Build();

app.MapGrpcService<UsersService>();

await app.InitAndRunAsync();