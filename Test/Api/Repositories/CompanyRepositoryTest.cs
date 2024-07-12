using Api.Repositories;
using Api.Utils;
using Api.Validators.Companies;
using Data;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Test.Factories;

namespace Test.Api.Repositories
{
	[TestClass]
	public class CompanyRepositoryTest
	{
		public TestContext TestContext { get; set; }
		private IPostCompanyValidator _validator;
		private ICompanyRepository _repository;
		private Container _container;
		private Random _random = new();


		[TestInitialize]
		public async Task Setup() {
			var dbInitializer = new CosmosDbInitializer(TestContext.Properties["CosmosDBConnection"]?.ToString(), TestContext.Properties["CosmosDb"]?.ToString());
			var host = new HostBuilder()
				.ConfigureFunctionsWebApplication()
				.ConfigureServices(services => Startup.ConfigureServices(services, dbInitializer, isCleanUp: true))
				.Build();
			var serviceProvider = host.Services;
			_container = serviceProvider.GetRequiredService<ICompanyContainer>().Container;
			_repository = serviceProvider.GetRequiredService<ICompanyRepository>();
		}

		[TestMethod]
		public async Task SelectConditionsAsync() {
			// ターゲットの差し込み
			var targetCompanies = CompanyFactory.Generate(10);
			await Task.WhenAll(targetCompanies.Select(company => _container.CreateItemAsync(company, new PartitionKey((int)company.Category))));

			// 実行
			var getCompanies = await _repository.SelectConditionsAsync(new() { });
			Assert.AreEqual(targetCompanies.Count, getCompanies.Count, "全取得数一致");
			foreach (var (testCompany, index) in targetCompanies.OrderByDescending(x => x.CreatedAt).Select((x, i) => (x, i))) {
				var expectedCompanyJson = JsonConvert.SerializeObject(testCompany);
				var actualCompanyJson = JsonConvert.SerializeObject(getCompanies[index]);
				Assert.AreEqual(expectedCompanyJson, actualCompanyJson, "内容一致");
			}

			var selectCompanyName = targetCompanies.OrderBy(x => _random.Next()).FirstOrDefault().Name.Substring(1, 3);
			getCompanies = await _repository.SelectConditionsAsync(new() { { "name", selectCompanyName } });
			Assert.AreEqual(targetCompanies.Where(x => x.Name.Contains(selectCompanyName)).Count(), getCompanies.Count, "全取得数一致");
			Assert.IsTrue(getCompanies.All(x => x.Name.Contains(selectCompanyName)), "Name は部分一致");

			var selectCompanyCategory = targetCompanies.OrderBy(x => _random.Next()).FirstOrDefault().Category;
			getCompanies = await _repository.SelectConditionsAsync(new() { { "category", ((int)selectCompanyCategory).ToString() } });
			Assert.AreEqual(targetCompanies.Where(x => x.Category == selectCompanyCategory).Count(), getCompanies.Count, "全取得数一致");
			Assert.IsTrue(getCompanies.All(x => x.Category == selectCompanyCategory), "category は完全一致");
		}

		[TestMethod]
		public async Task DeleteAsync() {
			//ダミーデータの差し込み
			var targetCompanies = CompanyFactory.Generate(10);
			await Task.WhenAll(targetCompanies.Select(company => _container.CreateItemAsync(company, new PartitionKey((int)company.Category))));

			// 実行
			var targetCompany = targetCompanies.OrderBy(x => _random.Next()).FirstOrDefault();
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
			var targetCompanies = CompanyFactory.Generate(10);
			await Task.WhenAll(targetCompanies.Select(company => _container.CreateItemAsync(company, new PartitionKey((int)company.Category))));
			// 実行
			var targetCompany = targetCompanies.OrderBy(x => _random.Next()).FirstOrDefault();
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
