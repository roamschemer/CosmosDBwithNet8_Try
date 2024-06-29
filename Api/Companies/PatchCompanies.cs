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

			string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
			var companyData = JsonSerializer.Deserialize<Company>(requestBody);
			if (companyData == null) {
				return new BadRequestObjectResult("Invalid request payload.");
			}

			var response = await container.PatchItemAsync<Company>(
				id,
				new PartitionKey(category),
				patchOperations: [
					PatchOperation.Replace("/name", companyData.Name) // ì¡íËÇÃçÄñ⁄ÇÃÇ›ÇçXêV
				]
			);
			//var response = await container.ReplaceItemAsync(companyData, id, new PartitionKey(category)); Ç‹ÇÈÇ≤Ç∆íuä∑Ç∑ÇÈÇ»ÇÁÇ±ÇÍÇ‡Ç†ÇË

			_logger.LogInformation($"{response.RequestCharge}RU è¡îÔÇµÇ‹ÇµÇΩ");

			return new OkObjectResult(response);
		}
	}
}