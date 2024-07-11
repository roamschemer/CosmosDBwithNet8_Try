using Api.Repositories;
using Api.Utils;
using Api.Validators.Companies;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var connectionString = Environment.GetEnvironmentVariable("CosmosDBConnection");
var databaseId = Environment.GetEnvironmentVariable("CosmosDb");
var dbInitializer = new CosmosDbInitializer(connectionString, databaseId);

var host = new HostBuilder()
	.ConfigureFunctionsWebApplication()
	.ConfigureServices(async services => {
		services.AddApplicationInsightsTelemetryWorkerService();
		services.ConfigureFunctionsApplicationInsights();
		services.AddSingleton<ICompanyRepository>(provider => new CompanyRepository(
			provider.GetRequiredService<ILogger<CompanyRepository>>(),
			dbInitializer.GetContainerAsync("companies", "/" + "category").GetAwaiter().GetResult()
		));
		services.AddSingleton<IPostCompanyValidator, PostCompanyValidator>();
	})
	.Build();

host.Run();
