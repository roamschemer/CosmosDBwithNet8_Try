using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Azure.Cosmos;

namespace Test.Api
{
	public static class DataBaseUtil
	{

		public static async Task<Database> CreateDatabase(string? connectionString, string? databaseId) {
			var client = new CosmosClientBuilder(connectionString)
				.WithSerializerOptions(new() {
					PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
				})
				.Build();
			return await client.CreateDatabaseIfNotExistsAsync(databaseId);
		}

		public static async Task<Container> CreateCleanContainer(Database database, string containerId, string partitionKeyPath) {
			var containerResponse = await database.CreateContainerIfNotExistsAsync(containerId, partitionKeyPath);
			if (containerResponse.StatusCode != System.Net.HttpStatusCode.Created) {
				await containerResponse.Container.DeleteContainerAsync();
				containerResponse = await database.CreateContainerAsync(containerId, partitionKeyPath);
			}
			return containerResponse.Container;
		}
	}
}
