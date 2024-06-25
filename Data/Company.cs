using System.Text.Json.Serialization;

namespace Data
{
	public class Company()
	{
		public enum CategoryDatas
		{
			Admin,
			User,
			PowerUser,
			Customer,
		}

		[JsonPropertyName("id")]
		public string? Id { get; set; }
		[JsonPropertyName("category_id")]
		public CategoryDatas Category { get; set; } = CategoryDatas.User;
		[JsonPropertyName("name")]
		public string? Name { get; set; }
	}
}
