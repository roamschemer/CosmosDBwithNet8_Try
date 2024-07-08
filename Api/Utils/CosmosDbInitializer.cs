using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Azure.Cosmos;

namespace Api.Utils
{
	public class CosmosDbInitializer
	{
		private readonly Database _database;

		/// <summary>
		/// データベースを取得または作成して、コンテナ取得準備をします
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
		/// コンテナを取得または作成します
		/// </summary>
		/// <param name="containerId">コンテナID</param>
		/// <param name="partitionKeyPath">/から始まるパーティションキー</param>
		/// <param name="isCleanUp">初期化する？</param>
		/// <returns>コンテナ</returns>
		public async Task<Container> GetContainerAsync(string containerId, string partitionKeyPath, bool isCleanUp = false) {
			//var throughputProperties = ThroughputProperties.CreateAutoscaleThroughput(autoscaleMaxThroughput: 1000); // オートスケールの場合
			var throughputProperties = ThroughputProperties.CreateManualThroughput(400); // 固定の場合
			var containerResponse = await _database.CreateContainerIfNotExistsAsync(new ContainerProperties(containerId, partitionKeyPath), throughputProperties);
			if (isCleanUp && containerResponse.StatusCode != System.Net.HttpStatusCode.Created) {
				await containerResponse.Container.DeleteContainerAsync();
				containerResponse = await _database.CreateContainerAsync(new ContainerProperties(containerId, partitionKeyPath), throughputProperties);
			}
			return containerResponse.Container;
		}

	}
}
