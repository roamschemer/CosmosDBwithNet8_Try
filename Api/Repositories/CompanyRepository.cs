using Data;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Logging;

namespace Api.Repositories
{
	public interface ICompanyRepository
	{
		public Task<List<Company>> SelectConditionsAsync(Dictionary<string, string> conditions);
	}

	public class CompanyRepository : ICompanyRepository
	{
		private readonly ILogger<CompanyRepository> _logger;
		private readonly Container _container;

		public CompanyRepository(ILogger<CompanyRepository> logger, Container container) {
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


	}
}
