using Api.Controllers.Companies;
using Api.Repositories;
using Api.Validators.Companies;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Api.Utils
{
	public class Startup
	{
		/// <summary>
		/// DIコンテナへの登録
		/// </summary>
		/// <param name="services"></param>
		/// <param name="dbInitializer"></param>
		public static void ConfigureServices(IServiceCollection services, CosmosDbInitializer dbInitializer) {
			services.AddApplicationInsightsTelemetryWorkerService();
			services.ConfigureFunctionsApplicationInsights();
			//Repository層
			services.AddSingleton<ICompanyRepository>(provider => new CompanyRepository(
				provider.GetRequiredService<ILogger<CompanyRepository>>(),
				dbInitializer.GetContainerAsync("companies", "/" + "category").GetAwaiter().GetResult()
			));
			//Validator層
			services.AddSingleton<IPostCompanyValidator, PostCompanyValidator>();
			//Controller層
			services.AddSingleton<IGetCompanies, GetCompanies>();
		}
	}
}
