using Api.HttpTriggers.Companies;
using Api.Repositories;
using Data;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace Api.Validators.Companies
{
	public interface IPostCompanyValidator
	{
		public IReadOnlyList<ValidationResult> Validate(Company company);
	}

	public class PostCompanyValidator : IPostCompanyValidator
	{
		private readonly ILogger<PostCompanyValidator> _logger;
		private readonly ICompanyRepository _companyRepository;

		public PostCompanyValidator(ILogger<PostCompanyValidator> logger, ICompanyRepository companyRepository) {
			_logger = logger;
			_companyRepository = companyRepository;
		}

		public IReadOnlyList<ValidationResult> Validate(Company company) {
			var results = new List<ValidationResult>();


			if (string.IsNullOrWhiteSpace(company.Name)) {
				var message = $"{nameof(Company.Name)} は必須です。";
				results.Add(new ValidationResult(message));
				_logger.LogInformation(message);
			}

			if (company.Category == null) {
				var message = $"{nameof(Company.Category)} は必須です。";
				results.Add(new ValidationResult($"{nameof(Company.Category)} は必須です。"));
				_logger.LogInformation(message);
			}

			if (company.Name == "つけもの") {
				var message = $"ただし{company.Name} テメーはダメだ。";
				results.Add(new ValidationResult($"ただし{company.Name} テメーはダメだ。"));
				_logger.LogInformation(message);
			}

			if (company.Name == "キマリ") {
				var message = $"{company.Name} は通さない。";
				results.Add(new ValidationResult($"{company.Name} は通さない。"));
				_logger.LogInformation(message);
			}

			return results;
		}
	}
}
