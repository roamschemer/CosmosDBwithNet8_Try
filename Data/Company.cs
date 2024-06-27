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
		public string Id { get; set; }

		[JsonConverter(typeof(JsonStringEnumConverter))]
		public CategoryDatas Category { get; set; } = CategoryDatas.User;

		public string Name { get; set; }

		public DateTime CreatedAt { get; set; }
	}
}
