using Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Api.Companies
{
	public class PatchCompanies
	{
		private readonly ILogger<PatchCompanies> _logger;
		private readonly CosmosClient _cosmosClient;

		public PatchCompanies(ILogger<PatchCompanies> logger, CosmosClient cosmosClient) {
			_logger = logger;
			_cosmosClient = cosmosClient;
		}

		[Function(nameof(PatchCompanies))]
		public async Task<IActionResult> Run(
			[HttpTrigger(AuthorizationLevel.Function, "patch", Route = "companies/{id}")] HttpRequest req, string id) {


			_logger.LogInformation("C# HTTP trigger function processed a request.");

			string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
			var company = JsonSerializer.Deserialize<Company>(requestBody);
			if (company == null) {
				return new BadRequestObjectResult("Invalid request payload.");
			}

			var container = _cosmosClient.GetContainer(Environment.GetEnvironmentVariable("CosmosDb"), "companies");

			var response = await container.UpsertItemAsync<Company>(company);
			_logger.LogInformation($"{response.RequestCharge}RU è¡îÔÇµÇ‹ÇµÇΩ");
			return new OkObjectResult(company);
		}
	}
}