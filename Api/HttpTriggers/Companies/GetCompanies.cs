using Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Api.HttpTriggers.Companies
{
	public class GetCompanies
	{
		private readonly ILogger<GetCompanies> _logger;
		private readonly CosmosClient _cosmosClient;

		public GetCompanies(ILogger<GetCompanies> logger, CosmosClient cosmosClient) {
			_logger = logger;
			_cosmosClient = cosmosClient;
		}

		[Function(nameof(GetCompanies))]
		public async Task<IActionResult> Run(
			[HttpTrigger(AuthorizationLevel.Function, "get", Route = "companies")] HttpRequest req) {
			_logger.LogInformation("C# HTTP trigger function processed a get request.");

			var name = req.Query["name"].ToString();
			var category = req.Query["category"].ToString();

			var container = _cosmosClient.GetContainer(Environment.GetEnvironmentVariable("CosmosDb"), "companies");
			var queryable = container.GetItemLinqQueryable<Company>()
				.Where(c => string.IsNullOrEmpty(name) || c.Name.Contains(name))
				.Where(c => string.IsNullOrEmpty(category) || c.Category.ToString() == category)
				.OrderByDescending(c => c.CreatedAt);
			var iterator = queryable.ToFeedIterator();

			var companies = new List<Company>();
			while (iterator.HasMoreResults) {
				var response = await iterator.ReadNextAsync();
				_logger.LogInformation($"{response.RequestCharge}RU è¡îÔÇµÇ‹ÇµÇΩ");
				companies.AddRange(response);
			}
			return new OkObjectResult(companies);
		}
	}
}