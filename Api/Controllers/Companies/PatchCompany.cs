using Api.Repositories;
using Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Api.Controllers.Companies
{
	public interface IPatchCompany
	{
		public Task<IActionResult> Run(HttpRequest req);
	}

	public class PatchCompany : IPatchCompany
	{
		private readonly ILogger<PatchCompany> _logger;
		private readonly ICompanyRepository _companyRepository;

		public PatchCompany(ILogger<PatchCompany> logger, ICompanyRepository companyRepository) {
			_logger = logger;
			_companyRepository = companyRepository;
		}

		[Function(nameof(PatchCompany))]
		public async Task<IActionResult> Run(
			[HttpTrigger(AuthorizationLevel.Function, "patch", Route = "companies")] HttpRequest req) {
			_logger.LogInformation("C# HTTP trigger function processed a patch request.");
			string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
			var companyData = JsonSerializer.Deserialize<Company>(requestBody);
			if (companyData == null) {
				return new BadRequestObjectResult("Invalid request payload.");
			}
			var response = await _companyRepository.PatchAsync(companyData);
			return new OkObjectResult(response);
		}
	}
}