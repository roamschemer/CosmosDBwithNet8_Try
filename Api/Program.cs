using Api.Utils;
using Microsoft.Extensions.Hosting;

var connectionString = Environment.GetEnvironmentVariable("CosmosDBConnection");
var databaseId = Environment.GetEnvironmentVariable("CosmosDb");
var dbInitializer = new CosmosDbInitializer(connectionString, databaseId);

var host = new HostBuilder()
	.ConfigureFunctionsWebApplication()
	.ConfigureServices(services => Startup.ConfigureServices(services, dbInitializer))
	.Build();

host.Run();
