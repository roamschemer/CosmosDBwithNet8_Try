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

namespace Api
{
	public class GetCompanies
	{
		private readonly ILogger<GetCompanies> _logger;

		public GetCompanies(ILogger<GetCompanies> logger) {
			_logger = logger;
		}

		[Function(nameof(GetCompanies))]
		public async Task<IActionResult> Run(
			[HttpTrigger(AuthorizationLevel.Function, "get", Route = "companies")] HttpRequest req,
			[CosmosDBInput(Connection = "CosmosDBConnection")] CosmosClient client) {
			_logger.LogInformation("C# HTTP trigger function processed a request.");

			var name = req.Query["name"].ToString();

			var databaseName = Environment.GetEnvironmentVariable("CosmosDb");

			var container = client.GetContainer(databaseName, "companies");

			IQueryable<Company> queryable = container.GetItemLinqQueryable<Company>()
				.Where(c => string.IsNullOrEmpty(name) || c.Name.Contains(name));

			var iterator = queryable.ToFeedIterator<Company>();

			var companies = new List<Company>();
			while (iterator.HasMoreResults) {
				var response = await iterator.ReadNextAsync();
				companies.AddRange(response);
			}

			return new OkObjectResult(companies);
		}
	}
}