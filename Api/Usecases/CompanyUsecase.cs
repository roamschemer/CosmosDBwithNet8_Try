using Api.Repositories;
using Data;
using Microsoft.Extensions.Logging;
using static Data.Company;

namespace Api.Usecases
{
	public interface ICompanyUsecase
	{
		public Task<List<Company>> SelectConditionsAsync(Dictionary<string, string> conditions);
		public Task<Company> DeleteAsync(string id, CategoryDatas? category);
		public Task<Company> CreateAsync(Company company);
		public Task<Company> PatchAsync(Company company);
	}

	public class CompanyUsecase(ILogger<ICompanyUsecase> logger, ICompanyRepository companyRepository) : ICompanyUsecase
	{
		private readonly ILogger<ICompanyUsecase> _logger = logger;
		private readonly ICompanyRepository _companyRepository = companyRepository;

		public async Task<List<Company>> SelectConditionsAsync(Dictionary<string, string> conditions) => await _companyRepository.SelectConditionsAsync(conditions);

		public async Task<Company> DeleteAsync(string id, CategoryDatas? category) => await _companyRepository.DeleteAsync(id, category);

		public async Task<Company> CreateAsync(Company company) => await companyRepository.CreateAsync(company);
		public async Task<Company> PatchAsync(Company company) => await companyRepository.PatchAsync(company);
	}
}
