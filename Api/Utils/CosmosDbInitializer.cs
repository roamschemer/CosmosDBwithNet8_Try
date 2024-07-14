using Microsoft.Azure.Cosmos;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Api.Utils
{
	public class CosmosDbInitializer
	{
		public enum ThroughputMode
		{
			AutoScale,
			Manual,
		}

		private readonly Database _database;

		/// <summary>
		/// データベースを取得または作成して、コンテナ取得準備をします
		/// </summary>
		/// <param name="connectionString">DB接続文字列</param>
		/// <param name="databaseId">データベース名</param>
		public CosmosDbInitializer(string connectionString, string databaseId) {
			var jsonSerializerOptions = new JsonSerializerOptions() {
				DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
			};
			var cosmosSystemTextJsonSerializer = new CosmosSystemTextJsonSerializer(jsonSerializerOptions);
			var cosmosClientOptions = new CosmosClientOptions() {
				Serializer = cosmosSystemTextJsonSerializer
			};
			var client = new CosmosClient(connectionString, cosmosClientOptions);
			var databaseResponse = client.CreateDatabaseIfNotExistsAsync(databaseId).GetAwaiter().GetResult();
			_database = databaseResponse.Database;
		}

		/// <summary>
		/// コンテナを取得または作成します。mode などの設定は作成時のみ反映されます。
		/// </summary>
		/// <param name="containerId">コンテナID</param>
		/// <param name="partitionKeyPath">/から始まるパーティションキー</param>
		/// <param name="mode">スループットのモード</param>
		/// <param name="maxThroughput">最大スループット 未指定の場合最小で作成される</param>
		/// <param name="isCleanUp">初期化する？</param>
		/// <returns>コンテナ</returns>
		public async Task<Container> GetContainerAsync(string containerId, string partitionKeyPath, ThroughputMode mode = ThroughputMode.Manual, int? maxThroughput = null, bool isCleanUp = false) {
			var throughputProperties = mode switch {
				ThroughputMode.AutoScale => ThroughputProperties.CreateAutoscaleThroughput(maxThroughput ?? 1000),
				ThroughputMode.Manual => ThroughputProperties.CreateManualThroughput(maxThroughput ?? 400),
				_ => throw new ArgumentOutOfRangeException(nameof(mode), $"不正なモード mode: {mode}")
			};
			var containerResponse = await _database.CreateContainerIfNotExistsAsync(new ContainerProperties(containerId, partitionKeyPath), throughputProperties);
			if (isCleanUp && containerResponse.StatusCode != System.Net.HttpStatusCode.Created) {
				await containerResponse.Container.DeleteContainerAsync();
				containerResponse = await _database.CreateContainerAsync(new ContainerProperties(containerId, partitionKeyPath), throughputProperties);
			}
			return containerResponse.Container;
		}

	}
}
