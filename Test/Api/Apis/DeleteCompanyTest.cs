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
		public void Setup() {
			var dbInitializer = new CosmosDbInitializer(TestContext.Properties["CosmosDBConnection"]?.ToString(), TestContext.Properties["CosmosDb"]?.ToString() + nameof(DeleteCompanyTest));
			var host = new HostBuilder()
				.ConfigureFunctionsWebApplication(worker => Startup.ConfigureFunctionsWebApplication(worker))
				.ConfigureServices(services => Startup.ConfigureServices(services, dbInitializer, isCleanUp: true))
				.Build();
			var serviceProvider = host.Services;
			_companyContainer = serviceProvider.GetRequiredService<ICompanyContainer>().Container;
			_deleteCompany = serviceProvider.GetRequiredService<IDeleteCompany>();
		}

		[TestMethod]
		public async Task Run_削除() {
			var targetCompanies = CompanyFactory.Generate(10);
			await Task.WhenAll(targetCompanies.Select(company => _companyContainer.CreateItemAsync(company, new PartitionKey(company.Id))));

			var targetCompany = targetCompanies.OrderBy(x => _random.Next()).FirstOrDefault();
			var getCompanyResponse = await _companyContainer.ReadItemAsync<Company>(targetCompany.Id, new PartitionKey(targetCompany.Id));
			Assert.IsNotNull(getCompanyResponse.Resource, "削除前の存在を確認");

			var httpRequest = new DefaultHttpContext().Request;
			var result = await _deleteCompany.Run(httpRequest, targetCompany.Id);

			Assert.IsNotNull(result);
			var okResult = result as OkObjectResult;
			Assert.IsNotNull(okResult);
			var getCompany = okResult.Value as Company;

			bool isDeleted = false;
			try {
				getCompanyResponse = await _companyContainer.ReadItemAsync<Company>(targetCompany.Id, new PartitionKey(targetCompany.Id));
			}
			catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound) {
				isDeleted = true;
			}
			Assert.IsTrue(isDeleted, "削除された事を確認");
		}
	}
}