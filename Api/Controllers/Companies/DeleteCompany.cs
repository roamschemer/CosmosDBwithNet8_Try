using Api.Usecases;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Api.Controllers.Companies
{
	public interface IDeleteCompany
	{
		public Task<IActionResult> Run(HttpRequest req, string id);
	}

	public class DeleteCompany(ILogger<DeleteCompany> logger, ICompanyUsecase companyUsecase) : IDeleteCompany
	{
		private readonly ILogger<DeleteCompany> _logger = logger;
		private readonly ICompanyUsecase _companyUsecase = companyUsecase;

		[Function(nameof(DeleteCompany))]
		public async Task<IActionResult> Run(
			[HttpTrigger(AuthorizationLevel.Function, "delete", Route = "companies/{id}")] HttpRequest req, string id) {
			_logger.LogInformation("C# HTTP trigger function processed a delete request.");
			var response = await _companyUsecase.DeleteAsync(id);
			return new OkObjectResult(response);
		}
	}
}