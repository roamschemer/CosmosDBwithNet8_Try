using Api.Repositories;
using Api.Utils;
using Api.Validators.Companies;
using Data;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Azure.Cosmos.Serialization.HybridRow;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using Test.Factories;
using Test.Utils;

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
			Assert.AreEqual(testCompanies.Count, companies.Count); // 全取得

			var targetCompany = testCompanies.OrderBy(x => _random.Next()).FirstOrDefault();
			var selectName = targetCompany.Name.Substring(1, 3);
			companies = await _repository.SelectConditionsAsync(new() { { "name", selectName } });
			Assert.IsTrue(companies.All(x => targetCompany.Name.Contains(selectName))); // Name は部分一致

			var selectCategory = targetCompany.Category;
			companies = await _repository.SelectConditionsAsync(new() { { "category", selectCategory.ToString() } });
			Assert.IsTrue(companies.All(x => targetCompany.Name.Contains(selectName))); // category は完全一致
		}

		[TestMethod]
		public async Task DeleteAsync() {
			//ダミーデータの差し込み
			var testCompanies = CompanyFactory.Generate(10);
			await Task.WhenAll(testCompanies.Select(company => _container.CreateItemAsync(company, new PartitionKey((int)company.Category))));

			// 実行
			var targetCompany = testCompanies.OrderBy(x => _random.Next()).FirstOrDefault();
			Assert.IsNotNull(await TestUtil.GetObjectByIdAsync<Company>(_container, targetCompany.Id)); // 削除前の存在を確認
			var company = await _repository.DeleteAsync(targetCompany.Id, (int)targetCompany.Category);
			Assert.IsNull(await TestUtil.GetObjectByIdAsync<Company>(_container, targetCompany.Id)); // 削除を確認 
		}


		[TestMethod]
		public async Task CreateAsync() {
			// 実行
			var targetCompany = CompanyFactory.Generate(1).FirstOrDefault();
			var company = await _repository.CreateAsync(targetCompany);
			Assert.IsNotNull(await TestUtil.GetObjectByIdAsync<Company>(_container, targetCompany.Id)); // 存在を確認
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
			var getCompany = await TestUtil.GetObjectByIdAsync<Company>(_container, targetCompany.Id);
			Assert.AreEqual(patchCompany.Name, getCompany.Name); // Name は差し変わる
			Assert.AreEqual(targetCompany.CreatedAt, getCompany.CreatedAt); //CreateAt は変わらない
		}
	}
}
