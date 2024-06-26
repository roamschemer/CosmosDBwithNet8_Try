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
	public class PostCompanies
	{
		private readonly ILogger<GetCompanies> _logger;
		private readonly CosmosClient _cosmosClient;

		public PostCompanies(ILogger<GetCompanies> logger, CosmosClient cosmosClient) {
			_logger = logger;
			_cosmosClient = cosmosClient;
		}

		[Function(nameof(PostCompanies))]
		public async Task<IActionResult> Run(
			[HttpTrigger(AuthorizationLevel.Function, "post", Route = "companies")] HttpRequest req) {
			_logger.LogInformation("C# HTTP trigger function processed a post request.");

			string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
			var company = JsonSerializer.Deserialize<Company>(requestBody);
			if (company == null) {
				return new BadRequestObjectResult("Invalid request payload.");
			}

			company.Id = Guid.NewGuid().ToString();
			company.CreatedAt = DateTime.UtcNow;

			var container = _cosmosClient.GetContainer(Environment.GetEnvironmentVariable("CosmosDb"), "companies");

			var response = await container.CreateItemAsync(company, new PartitionKey((int)company.Category));
			_logger.LogInformation($"{response.RequestCharge}RU 消費しました");
			return new OkObjectResult(company);
		}
	}
}