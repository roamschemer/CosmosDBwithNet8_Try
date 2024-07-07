using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Api.Repositories;

namespace Api.HttpTriggers.Companies
{
	public class GetCompanies
	{
		private readonly ILogger<GetCompanies> _logger;
		private readonly ICompanyRepository _companyRepository;

		public GetCompanies(ILogger<GetCompanies> logger, ICompanyRepository companyRepository) {
			_logger = logger;
			_companyRepository = companyRepository;
		}

		[Function(nameof(GetCompanies))]
		public async Task<IActionResult> Run(
			[HttpTrigger(AuthorizationLevel.Function, "get", Route = "companies")] HttpRequest req) {

			_logger.LogInformation("C# HTTP trigger function processed a get request.");

			var companies = await _companyRepository.SelectConditionsAsync(new(){
				{ "name", req.Query["name"].ToString() },
				{ "category", req.Query["category"].ToString() }
			});

			return new OkObjectResult(companies);
		}
	}
}