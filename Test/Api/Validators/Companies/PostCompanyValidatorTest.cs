using Api.Utils;
using Api.Validators.Companies;
using Data;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Test.Api.Validators.Companies
{
	[TestClass]
	public class PostCompanyValidatorTest
	{
		public TestContext TestContext { get; set; }
		private Container _companyContainer;
		private Random _random = new();
		private IPostCompanyValidator _validator;

		[TestInitialize]
		public void Setup() {
			var dbInitializer = new CosmosDbInitializer(TestContext.Properties["CosmosDBConnection"]?.ToString(), TestContext.Properties["CosmosDb"]?.ToString() + nameof(PostCompanyValidatorTest));
			var host = new HostBuilder()
				.ConfigureFunctionsWebApplication(worker => Startup.ConfigureFunctionsWebApplication(worker))
				.ConfigureServices(services => Startup.ConfigureServices(services, dbInitializer, isCleanUp: true))
				.Build();
			var serviceProvider = host.Services;
			_companyContainer = serviceProvider.GetRequiredService<ICompanyContainer>().Container;
			_validator = serviceProvider.GetRequiredService<IPostCompanyValidator>();
		}

		[TestMethod]
		public void Validate_正常() {
			var company = new Company { Name = "Company", Category = Company.CategoryDatas.User };
			var results = _validator.Validate(company);
			Assert.AreEqual(0, results.Count);
		}

		[TestMethod]
		public void Validate_Nameが空() {
			var company = new Company { Name = string.Empty, Category = Company.CategoryDatas.User };
			var results = _validator.Validate(company);
			Assert.AreEqual(1, results.Count);
		}

		[TestMethod]
		public void Validate_Categoryがnull() {
			var company = new Company { Name = "Company", Category = null };
			var results = _validator.Validate(company);
			Assert.AreEqual(1, results.Count);
		}

		[TestMethod]
		public void Validate_ただしつけものテメーはダメだ() {
			var company = new Company { Name = "つけもの", Category = Company.CategoryDatas.User };
			var results = _validator.Validate(company);
			Assert.AreEqual(1, results.Count);
		}

		[TestMethod]
		public void Validate_キマリは通さないi() {
			var company = new Company { Name = "キマリ", Category = Company.CategoryDatas.User };
			var results = _validator.Validate(company);
			Assert.AreEqual(1, results.Count);
		}

	}
}