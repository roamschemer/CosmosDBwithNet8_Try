using Data;
using Google.Protobuf.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Data.Company;

namespace Api.Companies
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
			_logger.LogInformation("C# HTTP trigger function processed a request.");

			var name = req.Query["name"].ToString();
			var category = req.Query["category"].ToString();
			//CategoryDatas? category = null;

			//if (Enum.TryParse(req.Query["category"].ToString(), out CategoryDatas categoryData)) {
			//	category = categoryData;
			//}

			var databaseName = Environment.GetEnvironmentVariable("CosmosDb");

			var container = _cosmosClient.GetContainer(databaseName, "companies");

			IQueryable<Company> queryable = container.GetItemLinqQueryable<Company>()
				.Where(c => string.IsNullOrEmpty(name) || c.Name.Contains(name))
				//.Where(c => category == null || c.Category == category)
				.Where(c => string.IsNullOrEmpty(category) || c.Category.ToString() == category)
				.OrderByDescending(c => c.CreatedAt);

			var iterator = queryable.ToFeedIterator();

			var companies = new List<Company>();
			while (iterator.HasMoreResults) {
				var response = await iterator.ReadNextAsync();
				companies.AddRange(response);
			}

			return new OkObjectResult(companies);
		}
	}
}