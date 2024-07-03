using Data;
using System.ComponentModel.DataAnnotations;

namespace Api.Validators.Companies
{
	public class PostCompanyValidator
	{
		public List<ValidationResult> Validate(Company company) {
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
