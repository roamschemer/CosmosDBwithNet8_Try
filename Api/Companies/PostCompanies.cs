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
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Api.Companies
{
	public class PostCompanies
	{
		private readonly ILogger<PostCompanies> _logger;

		public PostCompanies(ILogger<PostCompanies> logger) {
			_logger = logger;
		}

		[Function(nameof(PostCompanies))]
		public async Task<IActionResult> Run(
			[HttpTrigger(AuthorizationLevel.Function, "post", Route = "companies")] HttpRequest req,
			[CosmosDBInput(Connection = "CosmosDBConnection")] CosmosClient client) {
			_logger.LogInformation("C# HTTP trigger function processed a request.");

			string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
			var company = JsonSerializer.Deserialize<Company>(requestBody);
			if (company == null) {
				return new BadRequestObjectResult("Invalid request payload.");
			}

			company.Id = Guid.NewGuid().ToString();
			company.CreatedAt = DateTime.UtcNow;

			var databaseName = Environment.GetEnvironmentVariable("CosmosDb");

			var container = client.GetContainer(databaseName, "companies");

			await container.CreateItemAsync(company, new PartitionKey(company.Category.ToString()));

			return new OkObjectResult(company);
		}
	}
}