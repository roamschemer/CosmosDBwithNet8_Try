using Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace Api
{
	public class GetCompanies
	{
		private readonly ILogger<GetCompanies> _logger;

		public GetCompanies(ILogger<GetCompanies> logger) {
			_logger = logger;
		}

		[Function(nameof(GetCompanies))]
		public IActionResult Run(
			[HttpTrigger(AuthorizationLevel.Function, "get", Route = "companies")] HttpRequest req,
			[CosmosDBInput(
				databaseName: "%CosmosDb%",
				containerName: "companies",
				Connection  = "CosmosDBConnection",
				SqlQuery = "SELECT * FROM c")] IEnumerable<Company> companies

		) {
			_logger.LogInformation("C# HTTP trigger function processed a request.");
			//var random = new Random();
			//var companies = new List<Company>();
			//var categories = (Company.CategoryDatas[])Enum.GetValues(typeof(Company.CategoryDatas));
			//companies = Enumerable
			//	.Range(0, 10)
			//	.Select(_ => new Company() {
			//		Id = Guid.NewGuid(),
			//		Category = categories[random.Next(categories.Length)],
			//		Name = new string(Enumerable.Repeat("ABCDEFGabcdefg", 20).Select(s => s[random.Next(s.Length)]).ToArray())
			//	})
			//	.ToList();
			return new OkObjectResult(companies);
		}
	}
}
