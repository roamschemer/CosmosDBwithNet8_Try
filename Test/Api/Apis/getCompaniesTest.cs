using Api.Controllers.Companies;
using Api.Repositories;
using Api.Utils;
using Api.Validators.Companies;
using Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Test.Factories;

namespace Test.Api.Apis
{
	[TestClass]
	public class GetCompaniesTest
	{
		public TestContext TestContext { get; set; }
		private Container _companyContainer;
		private Random _random = new();
		private GetCompanies _getCompanies;

		[TestInitialize]
		public async Task Setup() {
			var dbInitializer = new CosmosDbInitializer(TestContext.Properties["CosmosDBConnection"]?.ToString(), TestContext.Properties["CosmosDb"]?.ToString());
			_companyContainer = await dbInitializer.GetContainerAsync("companies", "/" + "category", isCleanUp: true);
			var host = new HostBuilder()
				.ConfigureFunctionsWebApplication()
				.ConfigureServices(async services => {
					services.AddApplicationInsightsTelemetryWorkerService();
					services.ConfigureFunctionsApplicationInsights();
					services.AddSingleton<ICompanyRepository>(provider => new CompanyRepository(
						provider.GetRequiredService<ILogger<CompanyRepository>>(),
						dbInitializer.GetContainerAsync("companies", "/" + "category").GetAwaiter().GetResult()
					));
					services.AddSingleton<IPostCompanyValidator, PostCompanyValidator>();
					services.AddSingleton<GetCompanies>();
				})
				.Build();
			var serviceProvider = host.Services;
			_getCompanies = serviceProvider.GetRequiredService<GetCompanies>();
		}

		[TestMethod]
		public async Task Run_クエリパラメータなし() {
			var targetCompanies = CompanyFactory.Generate(10);
			await Task.WhenAll(targetCompanies.Select(company => _companyContainer.CreateItemAsync(company, new PartitionKey((int)company.Category))));

			var queryString = new Dictionary<string, StringValues> { };
			var httpRequest = new DefaultHttpContext().Request;
			httpRequest.Query = new QueryCollection(queryString);

			var result = await _getCompanies.Run(httpRequest);

			Assert.IsNotNull(result);
			var okResult = result as OkObjectResult;
			Assert.IsNotNull(okResult);
			var getCompanies = okResult.Value as List<Company>;
			Assert.AreEqual(targetCompanies.Count, getCompanies.Count, "取得数一致");
			foreach (var (testCompany, index) in targetCompanies.OrderByDescending(x => x.CreatedAt).Select((x, i) => (x, i))) {
				var expectedCompanyJson = JsonConvert.SerializeObject(testCompany);
				var actualCompanyJson = JsonConvert.SerializeObject(getCompanies[index]);
				Assert.AreEqual(expectedCompanyJson, actualCompanyJson, "内容一致");
			}
		}

		[TestMethod]
		public async Task Run_クエリパラメータ_name() {
			var targetCompanies = CompanyFactory.Generate(10);
			await Task.WhenAll(targetCompanies.Select(company => _companyContainer.CreateItemAsync(company, new PartitionKey((int)company.Category))));

			var selectCompanyName = targetCompanies.OrderBy(x => _random.Next()).FirstOrDefault().Name.Substring(1, 3);
			var queryString = new Dictionary<string, StringValues> {
				{ "name", selectCompanyName }
			};

			var httpRequest = new DefaultHttpContext().Request;
			httpRequest.Query = new QueryCollection(queryString);

			var result = await _getCompanies.Run(httpRequest);

			Assert.IsNotNull(result);
			var okResult = result as OkObjectResult;
			Assert.IsNotNull(okResult);
			var getCompanies = okResult.Value as List<Company>;
			Assert.AreEqual(targetCompanies.Where(x => x.Name.Contains(selectCompanyName)).Count(), getCompanies.Count, "全取得数一致");
			Assert.IsTrue(getCompanies.All(x => x.Name.Contains(selectCompanyName)), "Name は部分一致");
		}

		[TestMethod]
		public async Task Run_クエリパラメータ_category() {
			var targetCompanies = CompanyFactory.Generate(10);
			await Task.WhenAll(targetCompanies.Select(company => _companyContainer.CreateItemAsync(company, new PartitionKey((int)company.Category))));

			var selectCompanyCategory = targetCompanies.OrderBy(x => _random.Next()).FirstOrDefault().Category;
			var queryString = new Dictionary<string, StringValues> {
				{ "category", ((int)selectCompanyCategory).ToString() }
			};

			var httpRequest = new DefaultHttpContext().Request;
			httpRequest.Query = new QueryCollection(queryString);

			var result = await _getCompanies.Run(httpRequest);

			Assert.IsNotNull(result);
			var okResult = result as OkObjectResult;
			Assert.IsNotNull(okResult);
			var getCompanies = okResult.Value as List<Company>;
			Assert.AreEqual(targetCompanies.Where(x => x.Category == selectCompanyCategory).Count(), getCompanies.Count, "全取得数一致");
			Assert.IsTrue(getCompanies.All(x => x.Category == selectCompanyCategory), "category は完全一致");
		}
	}
}
