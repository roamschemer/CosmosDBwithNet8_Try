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
			[HttpTrigger(AuthorizationLevel.Function, "patch", Route = "companies/{cagegory}/{id}")] HttpRequest req, int category, string id) {


			_logger.LogInformation("C# HTTP trigger function processed a request.");

			var container = _cosmosClient.GetContainer(Environment.GetEnvironmentVariable("CosmosDb"), "companies");
			var response = await container.ReadItemAsync<Company>(id, new PartitionKey(category));
			_logger.LogInformation($"{response.RequestCharge}RU è¡îÔÇµÇ‹ÇµÇΩ");

			var company = response.Resource;
			string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
			var companyData = JsonSerializer.Deserialize<Company>(requestBody);
			if (companyData == null) {
				return new BadRequestObjectResult("Invalid request payload.");
			}

			company.Name = companyData.Name;

			response = await container.UpsertItemAsync<Company>(company);
			_logger.LogInformation($"{response.RequestCharge}RU è¡îÔÇµÇ‹ÇµÇΩ");
			return new OkObjectResult(company);
		}
	}
}