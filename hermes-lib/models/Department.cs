using System.Text.Json.Serialization;

namespace br.dev.optimus.hermes.lib.models
{
    public class Department
    {
        [JsonPropertyName("id")]
        public ulong? Id { get; set; }
        [JsonPropertyName("name")]
        public string? Name { get; set; }
        [JsonPropertyName("created_at")]
        public ulong? CreatedAt { get; set; }
        [JsonPropertyName("updated_at")]
        public ulong? UpdatedAt { get; set; }
    }
}
