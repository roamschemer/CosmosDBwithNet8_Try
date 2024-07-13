using Api.Usecases;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Api.Controllers.Companies
{
	public interface IGetCompanies
	{
		public Task<IActionResult> Run(HttpRequest req);
	}

	public class GetCompanies(ILogger<GetCompanies> logger, ICompanyUsecase companyUsecase) : IGetCompanies
	{
		private readonly ILogger<GetCompanies> _logger = logger;
		private readonly ICompanyUsecase _companyUsecase = companyUsecase;

		[Function(nameof(GetCompanies))]
		public async Task<IActionResult> Run(
			[HttpTrigger(AuthorizationLevel.Function, "get", Route = "companies")] HttpRequest req) {

			_logger.LogInformation("C# HTTP trigger function processed a get request.");

			var companies = await _companyUsecase.SelectConditionsAsync(new(){
				{ "name", req.Query["name"].ToString() },
				{ "category", req.Query["category"].ToString() }
			});

			return new OkObjectResult(companies);
		}
	}
}