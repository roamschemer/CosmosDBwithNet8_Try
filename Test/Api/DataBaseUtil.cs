using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test.Api
{
	public static class DataBaseUtil
	{

		public static async Task<Database> CreateCleanDatabase(string? connectionString, string? databaseId) {
			var client = new CosmosClientBuilder(connectionString)
				.WithSerializerOptions(new() {
					PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
				})
				.Build();
			var databaseResponse = await client.CreateDatabaseIfNotExistsAsync(databaseId);
			if (databaseResponse.StatusCode != System.Net.HttpStatusCode.Created) {
				await client.GetDatabase(databaseId).DeleteAsync();
			}
			await client.CreateDatabaseIfNotExistsAsync(databaseId);
			var database = (Database)await client.CreateDatabaseIfNotExistsAsync(databaseId);
			return database;
		}
	}
}
