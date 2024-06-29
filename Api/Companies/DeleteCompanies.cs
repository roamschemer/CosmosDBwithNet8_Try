using Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Api.Companies
{
	public class DeleteCompanies
	{
		private readonly ILogger<DeleteCompanies> _logger;
		private readonly CosmosClient _cosmosClient;

		public DeleteCompanies(ILogger<DeleteCompanies> logger, CosmosClient cosmosClient) {
			_logger = logger;
			_cosmosClient = cosmosClient;
		}

		[Function(nameof(DeleteCompanies))]
		public async Task<IActionResult> Run(
			[HttpTrigger(AuthorizationLevel.Function, "delete", Route = "companies/{category:int}/{id}")] HttpRequest req, int category, string id) {


			_logger.LogInformation("C# HTTP trigger function processed a delete request.");

			var container = _cosmosClient.GetContainer(Environment.GetEnvironmentVariable("CosmosDb"), "companies");

			var response = await container.DeleteItemAsync<Company>(id, new PartitionKey(category));

			_logger.LogInformation($"{response.RequestCharge}RU è¡îÔÇµÇ‹ÇµÇΩ");

			return new OkObjectResult(response);
		}
	}
}