using Data;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;

namespace Api.Repositories
{

	public class CompanyRepository
	{
		private readonly Container _container;

		public CompanyRepository(Container container) {
			_container = container;
		}

		public async Task<IReadOnlyList<Company>> SelectConditionsAsync(Dictionary<string, string> conditions) {

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
			}
			return companies;
		}
	}
}
