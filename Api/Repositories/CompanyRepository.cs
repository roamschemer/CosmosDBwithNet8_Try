using Data;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Logging;

namespace Api.Repositories
{
	public interface ICompanyRepository
	{
		public Task<List<Company>> SelectConditionsAsync(Dictionary<string, string> conditions);
		public Task<Company> Delete(string id, int category);
		public Task<Company> Post(Company company);
		public Task<Company> Patch(Company company, string id, int category);
	}

	public class CompanyRepository : ICompanyRepository
	{
		private readonly ILogger<ICompanyRepository> _logger;
		private readonly Container _container;

		public CompanyRepository(ILogger<ICompanyRepository> logger, Container container) {
			_logger = logger;
			_container = container;
		}

		public async Task<List<Company>> SelectConditionsAsync(Dictionary<string, string> conditions) {

			var name = conditions.ContainsKey("name") ? conditions["name"] : null;
			var category = conditions.ContainsKey("category") ? conditions["category"] : null;

			var queryable = _container.GetItemLinqQueryable<Company>()
				.Where(c => string.IsNullOrEmpty(name) || c.Name.Contains(name))
				.Where(c => string.IsNullOrEmpty(category) || c.Category.ToString() == category)
				.OrderByDescending(c => c.CreatedAt);
			var iterator = queryable.ToFeedIterator();
			var companies = new List<Company>();
			while (iterator.HasMoreResults) {
				var response = await iterator.ReadNextAsync();
				companies.AddRange(response);
				_logger.LogInformation($"{response.RequestCharge}RU 消費しました");
			}
			return companies;
		}

		public async Task<Company> Delete(string id, int category) {
			var response = await _container.DeleteItemAsync<Company>(id, new PartitionKey(category));
			_logger.LogInformation($"{response.RequestCharge}RU 消費しました");
			return response;
		}

		public async Task<Company> Post(Company company) {
			var response = await _container.CreateItemAsync(company, new PartitionKey((int)company.Category));
			_logger.LogInformation($"{response.RequestCharge}RU 消費しました");
			return response;
		}

		public async Task<Company> Patch(Company company, string id, int category) {
			var response = await _container.PatchItemAsync<Company>(
				id,
				new PartitionKey(category),
				patchOperations: [
					PatchOperation.Replace("/name", company.Name),	// 更新
					//他にも色々できる。
					//PatchOperation.Add("/color", "silver"),			// 追加
					//PatchOperation.Remove("/used"),					// 削除
					//PatchOperation.Increment("/price", 50.00),		// インクリメント
					//PatchOperation.Set("/tags", new string[] {}),		// 空の配列を設定
					//PatchOperation.Add("/tags/-", "featured-bikes")	// 配列の末尾に値を追加
				]
			);
			//var response = await container.ReplaceItemAsync(companyData, id, new PartitionKey(category)); まるごと置換するならこれもあり

			_logger.LogInformation($"{response.RequestCharge}RU 消費しました");
			return response;
		}

	}
}
