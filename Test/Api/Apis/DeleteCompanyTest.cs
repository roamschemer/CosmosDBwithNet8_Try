using Api.Controllers.Companies;
using Api.Utils;
using Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Test.Factories;

namespace Test.Api.Apis
{
	[TestClass]
	public class DeleteCompanyTest
	{
		public TestContext TestContext { get; set; }
		private Container _companyContainer;
		private Random _random = new();
		private IDeleteCompany _deleteCompany;

		[TestInitialize]
		public async Task Setup() {
			var dbInitializer = new CosmosDbInitializer(TestContext.Properties["CosmosDBConnection"]?.ToString(), TestContext.Properties["CosmosDb"]?.ToString());
			var host = new HostBuilder()
				.ConfigureFunctionsWebApplication()
				.ConfigureServices(services => Startup.ConfigureServices(services, dbInitializer, isCleanUp: true))
				.Build();
			var serviceProvider = host.Services;
			_companyContainer = serviceProvider.GetRequiredService<ICompanyContainer>().Container;
			_deleteCompany = serviceProvider.GetRequiredService<IDeleteCompany>();
		}

		[TestMethod]
		public async Task Run_削除() {
			var targetCompanies = CompanyFactory.Generate(10);
			await Task.WhenAll(targetCompanies.Select(company => _companyContainer.CreateItemAsync(company, new PartitionKey((int)company.Category))));

			var targetCompany = targetCompanies.OrderBy(x => _random.Next()).FirstOrDefault();
			var getCompanyResponse = await _companyContainer.ReadItemAsync<Company>(targetCompany.Id, new PartitionKey((int)targetCompany.Category));
			Assert.IsNotNull(getCompanyResponse.Resource, "削除前の存在を確認");

			var httpRequest = new DefaultHttpContext().Request;
			var result = await _deleteCompany.Run(httpRequest, targetCompany.Id, (int)targetCompany.Category);

			Assert.IsNotNull(result);
			var okResult = result as OkObjectResult;
			Assert.IsNotNull(okResult);
			var getCompanies = okResult.Value as Company;

			bool isDeleted = false;
			try {
				getCompanyResponse = await _companyContainer.ReadItemAsync<Company>(targetCompany.Id, new PartitionKey((int)targetCompany.Category));
			}
			catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound) {
				isDeleted = true;
			}
			Assert.IsTrue(isDeleted, "削除された事を確認");
		}
	}
}