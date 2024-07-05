using Api.HttpTriggers.Companies;
using Api.Repositories;
using Api.Validators.Companies;
using Data;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Extensions.Logging;
using Moq;

namespace Test.Api.Validators.Companies
{
	[TestClass]
	public class PostCompanyValidatorTest
	{
		private PostCompanyValidator _validator;

		[TestInitialize]
		public void Setup() {
			var mockLogger = new Mock<ILogger<PostCompanyValidator>>();
			var mockCosmosClient = new Mock<ICompanyRepository>();
			_validator = new PostCompanyValidator(mockLogger.Object, mockCosmosClient.Object);
		}

		[TestMethod]
		public void Validate_正常() {
			var company = new Company { Name = "Company", Category = Company.CategoryDatas.User };
			var results = _validator.Validate(company);
			Assert.AreEqual(0, results.Count);
		}

		[TestMethod]
		public void Validate_Nameが空() {
			var company = new Company { Name = string.Empty, Category = Company.CategoryDatas.User };
			var results = _validator.Validate(company);
			Assert.AreEqual(1, results.Count);
		}

		[TestMethod]
		public void Validate_Categoryがnull() {
			var company = new Company { Name = "Company", Category = null };
			var results = _validator.Validate(company);
			Assert.AreEqual(1, results.Count);
		}

		[TestMethod]
		public void Validate_ただしつけものテメーはダメだ() {
			var company = new Company { Name = "つけもの", Category = Company.CategoryDatas.User };
			var results = _validator.Validate(company);
			Assert.AreEqual(1, results.Count);
		}

		[TestMethod]
		public void Validate_キマリは通さないi() {
			var company = new Company { Name = "キマリ", Category = Company.CategoryDatas.User };
			var results = _validator.Validate(company);
			Assert.AreEqual(1, results.Count);
		}

	}
}