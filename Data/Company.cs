using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Data
{
	public class Company()
	{
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public enum CategoryDatas
		{
			Admin,
			User,
			PowerUser,
			Customer,
		}

		[JsonPropertyName("id")]
		public string? Id { get; set; }

		[Required]
		[JsonPropertyName("name")]
		public string? Name { get; set; }

		[JsonPropertyName("category")]
		public CategoryDatas? Category { get; set; } = CategoryDatas.User;

		[JsonPropertyName("createdAt")]
		public DateTime? CreatedAt { get; set; }

		[JsonPropertyName("updatedAt")]
		public DateTime? UpdatedAt { get; set; }
	}
}
