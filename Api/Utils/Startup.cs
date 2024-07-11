using Api.Controllers.Companies;
using Api.Repositories;
using Api.Validators.Companies;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using static Api.Utils.CosmosDbInitializer;

namespace Api.Utils
{
	public class Startup
	{
		/// <summary>
		/// DIコンテナへの登録
		/// </summary>
		/// <param name="services"></param>
		/// <param name="dbInitializer"></param>
		public static void ConfigureServices(IServiceCollection services, CosmosDbInitializer dbInitializer, ThroughputMode mode = ThroughputMode.Manual, int? maxThroughput = null, bool isCleanUp = false) {
			services.AddApplicationInsightsTelemetryWorkerService();
			services.ConfigureFunctionsApplicationInsights();
			//Container
			services.AddSingleton<ICompanyContainer>(provider => new CosmosContainerWrapper(dbInitializer.GetContainerAsync("companies", "/" + "category", mode, maxThroughput, isCleanUp).GetAwaiter().GetResult()));
			//Repository
			services.AddSingleton<ICompanyRepository, CompanyRepository>();
			//Validator
			services.AddSingleton<IPostCompanyValidator, PostCompanyValidator>();
			//Controller
			services.AddSingleton<IGetCompanies, GetCompanies>();
		}
	}
}
