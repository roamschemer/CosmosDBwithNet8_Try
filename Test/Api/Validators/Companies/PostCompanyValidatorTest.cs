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
		public void Validate_����() {
			var company = new Company { Name = "Company", Category = Company.CategoryDatas.User };
			var results = _validator.Validate(company);
			Assert.AreEqual(0, results.Count);
		}

		[TestMethod]
		public void Validate_Name����() {
			var company = new Company { Name = string.Empty, Category = Company.CategoryDatas.User };
			var results = _validator.Validate(company);
			Assert.AreEqual(1, results.Count);
		}

		[TestMethod]
		public void Validate_Category��null() {
			var company = new Company { Name = "Company", Category = null };
			var results = _validator.Validate(company);
			Assert.AreEqual(1, results.Count);
		}

		[TestMethod]
		public void Validate_�����������̃e���[�̓_����() {
			var company = new Company { Name = "������", Category = Company.CategoryDatas.User };
			var results = _validator.Validate(company);
			Assert.AreEqual(1, results.Count);
		}

		[TestMethod]
		public void Validate_�L�}���͒ʂ��Ȃ�i() {
			var company = new Company { Name = "�L�}��", Category = Company.CategoryDatas.User };
			var results = _validator.Validate(company);
			Assert.AreEqual(1, results.Count);
		}

	}
}