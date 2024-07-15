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

namespace Test.Api.Controllers.Companies
{
	[TestClass]
	public class PostCompanyTest
	{
		public TestContext TestContext { get; set; }
		private Container _companyContainer;
		private Random _random = new();
		private IPostCompany _postCompany;

		[TestInitialize]
		public void Setup() {
			var dbInitializer = new CosmosDbInitializer(TestContext.Properties["CosmosDBConnection"]?.ToString(), TestContext.Properties["CosmosDb"]?.ToString() + nameof(PostCompanyTest));
			var host = new HostBuilder()
				.ConfigureFunctionsWebApplication(worker => Startup.ConfigureFunctionsWebApplication(worker))
				.ConfigureServices(services => Startup.ConfigureServices(services, dbInitializer, isCleanUp: true))
				.Build();
			var serviceProvider = host.Services;
			_companyContainer = serviceProvider.GetRequiredService<ICompanyContainer>().Container;
			_postCompany = serviceProvider.GetRequiredService<IPostCompany>();
		}

		[TestMethod]
		public async Task Run_作成() {
			var targetCompany = CompanyFactory.Generate(1).FirstOrDefault();
			targetCompany.Id = null; //Post時にIDは存在しない

			var httpRequest = new DefaultHttpContext().Request;

			httpRequest.Body = new MemoryStream(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(targetCompany)));

			var result = await _postCompany.Run(httpRequest);

			Assert.IsNotNull(result);
			var okResult = result as OkObjectResult;
			Assert.IsNotNull(okResult);
			var getCompany = okResult.Value as Company;

			Assert.IsNotNull(getCompany.Id, "IDが付与されている");
			Assert.IsNotNull(getCompany.CreatedAt, "作成日が付与されている");
			var getCompanyResponse = await _companyContainer.ReadItemAsync<Company>(getCompany.Id, new PartitionKey(getCompany.Category.ToString()));
			Assert.IsNotNull(getCompanyResponse.Resource, "存在を確認");

		}
	}
}
