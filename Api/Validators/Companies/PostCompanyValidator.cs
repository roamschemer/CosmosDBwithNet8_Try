using Api.HttpTriggers.Companies;
using Data;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace Api.Validators.Companies
{
	public class PostCompanyValidator
	{

		public IReadOnlyList<ValidationResult> Validate(Company company) {
			var results = new List<ValidationResult>();


			if (string.IsNullOrWhiteSpace(company.Name)) {
				results.Add(new ValidationResult($"{nameof(Company.Name)} は必須です。"));
			}

			if (company.Category == null) {
				results.Add(new ValidationResult($"{nameof(Company.Category)} は必須です。"));
			}

			if (company.Name == "つけもの") {
				results.Add(new ValidationResult($"ただし{company.Name} テメーはダメだ。"));
			}

			if (company.Name == "キマリ") {
				results.Add(new ValidationResult($"{company.Name} は通さない。"));
			}

			return results;
		}
	}
}
