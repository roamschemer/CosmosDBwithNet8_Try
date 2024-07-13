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
		public void Validate_����() {
			var company = new Company { Name = "Company", Category = Company.CategoryDatas.User };
			var results = _validator.Validate(company);
			Assert.AreEqual(0, results.Count);
		}

		[TestMethod]
		public void Validate_Name����() {
			var company = new Company { Name = string.Empty, Category = Company.CategoryDatas.User };
			var results = _validator.Validate(company);
			Assert.AreEqual(1, results.Count);
		}

		[TestMethod]
		public void Validate_Category��null() {
			var company = new Company { Name = "Company", Category = null };
			var results = _validator.Validate(company);
			Assert.AreEqual(1, results.Count);
		}

		[TestMethod]
		public void Validate_�����������̃e���[�̓_����() {
			var company = new Company { Name = "������", Category = Company.CategoryDatas.User };
			var results = _validator.Validate(company);
			Assert.AreEqual(1, results.Count);
		}

		[TestMethod]
		public void Validate_�L�}���͒ʂ��Ȃ�i() {
			var company = new Company { Name = "�L�}��", Category = Company.CategoryDatas.User };
			var results = _validator.Validate(company);
			Assert.AreEqual(1, results.Count);
		}

	}
}