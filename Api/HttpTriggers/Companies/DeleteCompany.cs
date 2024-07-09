using Api.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Api.HttpTriggers.Companies
{
	public class DeleteCompany
	{
		private readonly ILogger<DeleteCompany> _logger;
		private readonly ICompanyRepository _companyRepository;

		public DeleteCompany(ILogger<DeleteCompany> logger, ICompanyRepository companyRepository) {
			_logger = logger;
			_companyRepository = companyRepository;
		}

		[Function(nameof(DeleteCompany))]
		public async Task<IActionResult> Run(
			[HttpTrigger(AuthorizationLevel.Function, "delete", Route = "companies/{category:int}/{id}")] HttpRequest req, int category, string id) {
			_logger.LogInformation("C# HTTP trigger function processed a delete request.");
			var response = await _companyRepository.DeleteAsync(id, category);
			return new OkObjectResult(response);
		}
	}
}