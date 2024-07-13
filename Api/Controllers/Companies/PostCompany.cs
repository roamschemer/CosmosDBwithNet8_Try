using Api.Usecases;
using Api.Validators.Companies;
using Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Api.Controllers.Companies
{
	public interface IPostCompany
	{
		public Task<IActionResult> Run(HttpRequest req);
	}

	public class PostCompany(ILogger<GetCompanies> logger, IPostCompanyValidator validator, ICompanyUsecase companyUsecase) : IPostCompany
	{
		private readonly ILogger<GetCompanies> _logger = logger;
		private readonly IPostCompanyValidator _validator = validator;
		private readonly ICompanyUsecase _companyUsecase = companyUsecase;

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

			var response = await _companyUsecase.CreateAsync(company);
			return new OkObjectResult(company);
		}
	}
}