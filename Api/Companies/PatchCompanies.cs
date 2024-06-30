using Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Api.Companies
{
	public class PatchCompanies
	{
		private readonly ILogger<PatchCompanies> _logger;
		private readonly CosmosClient _cosmosClient;

		public PatchCompanies(ILogger<PatchCompanies> logger, CosmosClient cosmosClient) {
			_logger = logger;
			_cosmosClient = cosmosClient;
		}

		[Function(nameof(PatchCompanies))]
		public async Task<IActionResult> Run(
			[HttpTrigger(AuthorizationLevel.Function, "patch", Route = "companies/{category:int}/{id}")] HttpRequest req, int category, string id) {


			_logger.LogInformation("C# HTTP trigger function processed a patch request.");

			var container = _cosmosClient.GetContainer(Environment.GetEnvironmentVariable("CosmosDb"), "companies");

			string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
			var companyData = JsonSerializer.Deserialize<Company>(requestBody);
			if (companyData == null) {
				return new BadRequestObjectResult("Invalid request payload.");
			}

			var response = await container.PatchItemAsync<Company>(
				id,
				new PartitionKey(category),
				patchOperations: [
					PatchOperation.Replace("/name", companyData.Name),	// �X�V
					//���ɂ��F�X�ł���B
					//PatchOperation.Add("/color", "silver"),			// �ǉ�
					//PatchOperation.Remove("/used"),					// �폜
					//PatchOperation.Increment("/price", 50.00),		// �C���N�������g
					//PatchOperation.Set("/tags", new string[] {}),		// ��̔z���ݒ�
					//PatchOperation.Add("/tags/-", "featured-bikes")	// �z��̖����ɒl��ǉ�
				]
			);
			//var response = await container.ReplaceItemAsync(companyData, id, new PartitionKey(category)); �܂邲�ƒu������Ȃ炱�������

			_logger.LogInformation($"{response.RequestCharge}RU ����܂���");

			return new OkObjectResult(response);
		}
	}
}