using System.Text.Json.Serialization;

namespace Data
{
	public class Company()
	{
		[JsonPropertyName("id")]
		public Guid? Id { get; set; }
		[JsonPropertyName("name")]
		public string? Name { get; set; }
	}
}
