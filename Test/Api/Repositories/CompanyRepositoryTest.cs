using Api.Validators.Companies;

namespace Test.Api.Repositories
{
	[TestClass]
	public class CompanyRepositoryTest
	{
		public TestContext TestContext { get; set; }
		private IPostCompanyValidator _validator;

		[TestInitialize]
		public void Setup() {
			var connectionString = TestContext.Properties["CosmosDBConnection"];
			var databaseId = TestContext.Properties["CosmosDb"];
		}

		[TestMethod]
		public void Validate_正常() {
		}

	}
}
