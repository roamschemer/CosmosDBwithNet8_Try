using Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
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

			var validationResults = new List<ValidationResult>();
			var validationContext = new ValidationContext(company, null, null);

			if (!Validator.TryValidateObject(company, validationContext, validationResults, true)) {
				return new BadRequestObjectResult(validationResults);
			}

			company.Id = Guid.NewGuid().ToString();
			company.CreatedAt = DateTime.UtcNow;

			var container = _cosmosClient.GetContainer(Environment.GetEnvironmentVariable("CosmosDb"), "companies");

			var response = await container.CreateItemAsync(company, new PartitionKey((int)company.Category));
			_logger.LogInformation($"{response.RequestCharge}RU è¡îÔÇµÇ‹ÇµÇΩ");
			return new OkObjectResult(company);
		}
	}
}