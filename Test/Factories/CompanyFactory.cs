using Bogus;
using Data;

namespace Test.Factories
{
	public static class CompanyFactory
	{
		public static List<Company> Generate(int count = 1) {
			var fakerCompany = new Faker<Company>("ja")
				.StrictMode(true)
				.RuleFor(o => o.Id, f => Guid.NewGuid().ToString())
				.RuleFor(o => o.Name, f => f.Company.CompanyName())
				.RuleFor(o => o.Category, f => f.PickRandom<Company.CategoryDatas>())
				.RuleFor(o => o.CreatedAt, f => f.Date.Past(2));

			return fakerCompany.Generate(count);
		}
	}
}
