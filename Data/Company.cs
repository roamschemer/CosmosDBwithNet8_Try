﻿using System.Text.Json.Serialization;

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

		[JsonPropertyName("name")]
		public string Name { get; set; }

		[JsonPropertyName("category")]
		//[JsonConverter(typeof(JsonStringEnumConverter))] //効かない様子なのでコメント化しておく
		public CategoryDatas Category { get; set; } = CategoryDatas.User;


		[JsonPropertyName("createdAt")]
		public DateTime CreatedAt { get; set; }
	}
}
