using Api.Repositories;
using Api.Validators.Companies;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var host = new HostBuilder()
	.ConfigureFunctionsWebApplication()
	.ConfigureServices((Action<IServiceCollection>)(services => {
		services.AddApplicationInsightsTelemetryWorkerService();
		services.ConfigureFunctionsApplicationInsights();
		RepositoryInjection(services);
		ValidatorsInjection(services);
	}))
	.Build();

host.Run();

static void RepositoryInjection(IServiceCollection services) {
	var connectionString = Environment.GetEnvironmentVariable("CosmosDBConnection");
	var client = new CosmosClientBuilder(connectionString)
		.WithSerializerOptions(new() {
			PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
		})
		.Build();
	var databaseId = Environment.GetEnvironmentVariable("CosmosDb");
	services.AddSingleton<ICompanyRepository>(provider => new CompanyRepository(
		provider.GetRequiredService<ILogger<CompanyRepository>>(),
		client.GetContainer(databaseId, "companies"))
	);
}

static void ValidatorsInjection(IServiceCollection services) {
	services.AddSingleton<IPostCompanyValidator, PostCompanyValidator>();
}