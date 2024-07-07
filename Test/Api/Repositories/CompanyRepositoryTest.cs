using Api.Repositories;
using Api.Validators.Companies;
using Data;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Extensions.Logging;
using Moq;

namespace Test.Api.Repositories
{
	[TestClass]
	public class CompanyRepositoryTest
	{
		public TestContext? TestContext { get; set; }
		private IPostCompanyValidator? _validator;
		private CompanyRepository? _repository;
		private Container? _container;

		[TestInitialize]
		public async Task Setup() {
			var mockLogger = new Mock<ILogger<ICompanyRepository>>();
			var connectionString = TestContext?.Properties["CosmosDBConnection"]?.ToString();
			var databaseId = TestContext?.Properties["CosmosDb"]?.ToString();
			var client = new CosmosClientBuilder(connectionString)
				.WithSerializerOptions(new() {
					PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
				})
				.Build();
			var databaseResponse = await client.CreateDatabaseIfNotExistsAsync(databaseId);
			if (databaseResponse.StatusCode != System.Net.HttpStatusCode.Created) {
				await client.GetDatabase(databaseId).DeleteAsync();
			}
			await client.CreateDatabaseIfNotExistsAsync(databaseId);
			var database = (Database)await client.CreateDatabaseIfNotExistsAsync(databaseId);
			_container = await database.CreateContainerIfNotExistsAsync("companies", "/category");
			_repository = new CompanyRepository(mockLogger.Object, _container);
		}

		[TestMethod]
		public async Task SelectConditionsAsync() {
			// テストデータ挿入
			var testCompany = new Company {
				Id = Guid.NewGuid().ToString(),
				Name = "TestCompany",
				Category = Company.CategoryDatas.Admin,
				CreatedAt = DateTime.UtcNow
			};
			await _container.CreateItemAsync(testCompany, new PartitionKey((int)testCompany.Category));

			// 実行
			var companies = await _repository.SelectConditionsAsync(new());
		}

	}
}
