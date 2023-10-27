using System.Text.Json.Serialization;

namespace br.dev.optimus.hermes.lib.models
{
    public class DocumentFile
    {
        [JsonPropertyName("path")]
        public string? Path { get; set; }
        [JsonPropertyName("name")]
        public string? Name { get; set; }
        [JsonPropertyName("page")]
        public uint? Page { get; set; }
        [JsonPropertyName("error")]
        public string? Error { get; set; }
    }
}
