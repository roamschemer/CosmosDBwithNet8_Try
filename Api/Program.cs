using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text.Json;

var host = new HostBuilder()
	.ConfigureFunctionsWebApplication()
	.ConfigureServices(services => {
		services.AddApplicationInsightsTelemetryWorkerService();
		services.ConfigureFunctionsApplicationInsights();
		services.AddSingleton(provider => {
			CosmosSerializationOptions serializerOptions = new() {
				PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
			};
			var connectionString = Environment.GetEnvironmentVariable("CosmosDBConnection");
			var client = new CosmosClientBuilder(connectionString)
				.WithSerializerOptions(serializerOptions)
				.Build();
			return client;
		});

	})
	.Build();

host.Run();
