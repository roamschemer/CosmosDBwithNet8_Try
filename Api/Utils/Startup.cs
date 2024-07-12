using Api.Controllers.Companies;
using Api.Middlewares;
using Api.Repositories;
using Api.Validators.Companies;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
			services.AddSingleton<IDeleteCompany, DeleteCompany>();
			services.AddSingleton<IPatchCompany, PatchCompany>();
			services.AddSingleton<IPostCompany, PostCompany>();
		}

		/// <summary>
		/// ミドルウェアの登録
		/// </summary>
		/// <param name="builder"></param>
		/// <returns></returns>
		public static void ConfigureFunctionsWebApplication(IFunctionsWorkerApplicationBuilder worker) {
			worker.UseMiddleware<LoggingMiddleware>();
		}
	}
}
