using Api.Validators.Companies;
using Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Api.HttpTriggers.Companies
{
	public class PostCompany
	{
		private readonly ILogger<GetCompanies> _logger;
		private readonly PostCompanyValidator _validator;

		public PostCompany(ILogger<GetCompanies> logger, PostCompanyValidator validator) {
			_logger = logger;
			_validator = validator;
		}

		[Function(nameof(PostCompany))]
		public async Task<IActionResult> Run(
			[HttpTrigger(AuthorizationLevel.Function, "post", Route = "companies")] HttpRequest req) {
			_logger.LogInformation("C# HTTP trigger function processed a post request.");

			string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
			var company = JsonSerializer.Deserialize<Company>(requestBody);
			if (company == null) {
				return new BadRequestObjectResult("Invalid request payload.");
			}

			var validationResults = _validator.Validate(company);

			if (validationResults.Any()) {
				return new BadRequestObjectResult(validationResults);
			}

			company.Id = Guid.NewGuid().ToString();
			company.CreatedAt = DateTime.UtcNow;

			//var container = _cosmosClient.GetContainer(Environment.GetEnvironmentVariable("CosmosDb"), "companies");

			//var response = await container.CreateItemAsync(company, new PartitionKey((int)company.Category));
			//_logger.LogInformation($"{response.RequestCharge}RU è¡îÔÇµÇ‹ÇµÇΩ");
			return new OkObjectResult(company);
		}
	}
}