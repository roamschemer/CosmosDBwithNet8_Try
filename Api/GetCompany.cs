using Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Api
{
	public class GetCompany
	{
		private readonly ILogger<GetCompany> _logger;

		public GetCompany(ILogger<GetCompany> logger) {
			_logger = logger;
		}

		[Function("GetCompany")]
		public IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req) {
			_logger.LogInformation("C# HTTP trigger function processed a request.");
			var random = new Random();
			var name = new string(Enumerable.Repeat("ABCDEFGabcdefg", 20)
											.Select(s => s[random.Next(s.Length)]).ToArray());
			var company = new Company(name);
			return new OkObjectResult(company);
		}
	}
}
