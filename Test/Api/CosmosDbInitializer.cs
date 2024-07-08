using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Azure.Cosmos;

namespace Test.Api
{
	public class CosmosDbInitializer
	{
		private readonly Database _database;

		/// <summary>
		/// データベースを作成します
		/// </summary>
		/// <param name="connectionString">DB接続文字列</param>
		/// <param name="databaseId">データベース名</param>
		public CosmosDbInitializer(string connectionString, string databaseId) {
			var client = new CosmosClientBuilder(connectionString)
				.WithSerializerOptions(new() {
					PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
				})
				.Build();
			var databaseResponse = client.CreateDatabaseIfNotExistsAsync(databaseId).GetAwaiter().GetResult();
			_database = databaseResponse.Database;
		}

		/// <summary>
		/// 空のコンテナを作成します
		/// </summary>
		/// <param name="containerId">コンテナID</param>
		/// <param name="partitionKeyPath">パーティションキー</param>
		/// <returns></returns>
		public async Task<Container> CreateCleanContainer(string containerId, string partitionKeyPath) {
			var containerResponse = await _database.CreateContainerIfNotExistsAsync(containerId, partitionKeyPath);
			if (containerResponse.StatusCode != System.Net.HttpStatusCode.Created) {
				await containerResponse.Container.DeleteContainerAsync();
				containerResponse = await _database.CreateContainerAsync(containerId, partitionKeyPath);
			}
			return containerResponse.Container;
		}
	}
}
