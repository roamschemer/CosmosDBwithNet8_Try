using Api.Repositories;
using Api.Validators.Companies;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
	.ConfigureFunctionsWebApplication()
	.ConfigureServices(services => {
		services.AddApplicationInsightsTelemetryWorkerService();
		services.ConfigureFunctionsApplicationInsights();
		var connectionString = Environment.GetEnvironmentVariable("CosmosDBConnection");
		var client = new CosmosClientBuilder(connectionString)
			.WithSerializerOptions(new() {
				PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
			})
			.Build();
		services.AddSingleton(provider => new CompanyRepository(client.GetContainer(Environment.GetEnvironmentVariable("CosmosDb"), "companies")));
		//Validator
		services.AddSingleton<PostCompanyValidator>();
	})
	.Build();

host.Run();
