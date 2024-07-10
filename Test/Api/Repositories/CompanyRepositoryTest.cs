using Api.Repositories;
using Api.Utils;
using Api.Validators.Companies;
using Data;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Moq;
using Test.Factories;

namespace Test.Api.Repositories
{
	[TestClass]
	public class CompanyRepositoryTest
	{
		public TestContext TestContext { get; set; }
		private IPostCompanyValidator _validator;
		private CompanyRepository _repository;
		private Container _container;
		private Random _random = new();


		[TestInitialize]
		public async Task Setup() {
			var mockLogger = new Mock<ILogger<ICompanyRepository>>();
			var dbInitializer = new CosmosDbInitializer(TestContext.Properties["CosmosDBConnection"]?.ToString(), TestContext.Properties["CosmosDb"]?.ToString());
			_container = await dbInitializer.GetContainerAsync("companies", "/" + "category", isCleanUp: true);
			_repository = new CompanyRepository(mockLogger.Object, _container);
		}

		[TestMethod]
		public async Task SelectConditionsAsync() {
			//ダミーデータの差し込み
			var testCompanies = CompanyFactory.Generate(10);
			await Task.WhenAll(testCompanies.Select(company => _container.CreateItemAsync(company, new PartitionKey((int)company.Category))));

			// 実行
			var companies = await _repository.SelectConditionsAsync(new() { });
			Assert.AreEqual(testCompanies.Count, companies.Count, "全取得");

			var targetCompany = testCompanies.OrderBy(x => _random.Next()).FirstOrDefault();
			var selectName = targetCompany.Name.Substring(1, 3);
			companies = await _repository.SelectConditionsAsync(new() { { "name", selectName } });
			Assert.IsTrue(companies.All(x => targetCompany.Name.Contains(selectName)), "Name は部分一致");

			var selectCategory = targetCompany.Category;
			companies = await _repository.SelectConditionsAsync(new() { { "category", selectCategory.ToString() } });
			Assert.IsTrue(companies.All(x => targetCompany.Name.Contains(selectName)), "category は完全一致");
		}

		[TestMethod]
		public async Task DeleteAsync() {
			//ダミーデータの差し込み
			var testCompanies = CompanyFactory.Generate(10);
			await Task.WhenAll(testCompanies.Select(company => _container.CreateItemAsync(company, new PartitionKey((int)company.Category))));

			// 実行
			var targetCompany = testCompanies.OrderBy(x => _random.Next()).FirstOrDefault();
			var getCompanyResponse = await _container.ReadItemAsync<Company>(targetCompany.Id, new PartitionKey((int)targetCompany.Category));
			Assert.IsNotNull(getCompanyResponse.Resource, "削除前の存在を確認");
			var company = await _repository.DeleteAsync(targetCompany.Id, (int)targetCompany.Category);
			bool isDeleted = false;
			try {
				getCompanyResponse = await _container.ReadItemAsync<Company>(targetCompany.Id, new PartitionKey((int)targetCompany.Category));
			}
			catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound) {
				isDeleted = true;
			}
			Assert.IsTrue(isDeleted, "削除された事を確認");
		}


		[TestMethod]
		public async Task CreateAsync() {
			// 実行
			var targetCompany = CompanyFactory.Generate(1).FirstOrDefault();
			var company = await _repository.CreateAsync(targetCompany);
			var getCompanyResponse = await _container.ReadItemAsync<Company>(targetCompany.Id, new PartitionKey((int)targetCompany.Category));
			Assert.IsNotNull(getCompanyResponse.Resource, "存在を確認");
		}

		[TestMethod]
		public async Task PatchAsync() {
			//ダミーデータの差し込み
			var testCompanies = CompanyFactory.Generate(10);
			await Task.WhenAll(testCompanies.Select(company => _container.CreateItemAsync(company, new PartitionKey((int)company.Category))));
			// 実行
			var targetCompany = testCompanies.OrderBy(x => _random.Next()).FirstOrDefault();
			var patchCompany = CompanyFactory.Generate(10).FirstOrDefault();
			patchCompany.Id = targetCompany.Id;
			patchCompany.Category = targetCompany.Category;
			var company = await _repository.PatchAsync(patchCompany);
			var getCompanyResponse = await _container.ReadItemAsync<Company>(targetCompany.Id, new PartitionKey((int)targetCompany.Category));
			var getCompany = getCompanyResponse.Resource;
			Assert.AreEqual(patchCompany.Name, getCompany.Name, "Name は差し変わる");
			Assert.AreEqual(targetCompany.CreatedAt, getCompany.CreatedAt, "CreateAt は変わらない");
		}
	}
}
