using Newtonsoft.Json;

namespace Data
{
    public class Company(string name)
    {
        [JsonProperty(PropertyName = "id")]
        public Guid? Id { get; set; }
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; } = name;
    }
}
