using Api.Controllers.Companies;
using Api.Middlewares;
using Api.Repositories;
using Api.Usecases;
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
			services.AddSingleton<ICompanyContainer>(provider => new CosmosContainerWrapper(dbInitializer.GetContainerAsync("companies", "/" + "id", mode, maxThroughput, isCleanUp).GetAwaiter().GetResult()));
			//Controller
			services.AddSingleton<IGetCompanies, GetCompanies>();
			services.AddSingleton<IDeleteCompany, DeleteCompany>();
			services.AddSingleton<IPatchCompany, PatchCompany>();
			services.AddSingleton<IPostCompany, PostCompany>();
			//Usecase
			services.AddSingleton<ICompanyUsecase, CompanyUsecase>();
			//Repository
			services.AddSingleton<ICompanyRepository, CompanyRepository>();
			//Validator
			services.AddSingleton<IPostCompanyValidator, PostCompanyValidator>();
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
