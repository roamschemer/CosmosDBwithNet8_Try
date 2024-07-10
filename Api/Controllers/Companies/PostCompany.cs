using Api.Repositories;
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
	public class PostCompany
	{
		private readonly ILogger<GetCompanies> _logger;
		private readonly IPostCompanyValidator _validator;
		private readonly ICompanyRepository _companyRepository;

		public PostCompany(ILogger<GetCompanies> logger, IPostCompanyValidator validator, ICompanyRepository companyRepository) {
			_logger = logger;
			_validator = validator;
			_companyRepository = companyRepository;
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

			var response = await _companyRepository.CreateAsync(company);
			return new OkObjectResult(company);
		}
	}
}