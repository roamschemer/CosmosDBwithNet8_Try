using Api.Controllers.Companies;
using Api.Utils;
using Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text;
using System.Text.Json;
using Test.Factories;

namespace Test.Api.Apis
{
	[TestClass]
	public class PatchCompanyTest
	{
		public TestContext TestContext { get; set; }
		private Container _companyContainer;
		private Random _random = new();
		private IPatchCompany _patchCompany;

		[TestInitialize]
		public void Setup() {
			var dbInitializer = new CosmosDbInitializer(TestContext.Properties["CosmosDBConnection"]?.ToString(), TestContext.Properties["CosmosDb"]?.ToString() + nameof(PatchCompanyTest));
			var host = new HostBuilder()
				.ConfigureFunctionsWebApplication(worker => Startup.ConfigureFunctionsWebApplication(worker))
				.ConfigureServices(services => Startup.ConfigureServices(services, dbInitializer, isCleanUp: true))
				.Build();
			var serviceProvider = host.Services;
			_companyContainer = serviceProvider.GetRequiredService<ICompanyContainer>().Container;
			_patchCompany = serviceProvider.GetRequiredService<IPatchCompany>();
		}

		[TestMethod]
		public async Task Run_更新() {
			var targetCompanies = CompanyFactory.Generate(10);
			await Task.WhenAll(targetCompanies.Select(company => _companyContainer.CreateItemAsync(company, new PartitionKey((int)company.Category))));

			var targetCompany = targetCompanies.OrderBy(x => _random.Next()).FirstOrDefault();

			var patchCompany = CompanyFactory.Generate(1).FirstOrDefault();
			patchCompany.Id = targetCompany.Id;
			patchCompany.Category = targetCompany.Category;

			var httpRequest = new DefaultHttpContext().Request;
			httpRequest.Body = new MemoryStream(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(patchCompany)));

			var result = await _patchCompany.Run(httpRequest);

			Assert.IsNotNull(result);
			var okResult = result as OkObjectResult;
			Assert.IsNotNull(okResult);

			var getCompanyResponse = await _companyContainer.ReadItemAsync<Company>(targetCompany.Id, new PartitionKey((int)targetCompany.Category));
			var getCompany = getCompanyResponse.Resource;
			Assert.AreEqual(patchCompany.Name, getCompany.Name, "Name は差し変わる");
			Assert.AreEqual(targetCompany.CreatedAt, getCompany.CreatedAt, "CreateAt は変わらない");
			Assert.AreNotEqual(patchCompany.UpdatedAt, getCompany.UpdatedAt, "UpdatedAt は更新される");

		}
	}
}
