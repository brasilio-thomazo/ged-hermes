using br.dev.optimus.hermes.lib.responses;
using System.Text.Json.Serialization;

namespace br.dev.optimus.hermes.lib.models
{
    public class DocumentImage
    {
        [JsonPropertyName("id")]
        public ulong? Id { get; set; }
        [JsonPropertyName("document_id")]
        public string? DocumentId { get; set; }
        [JsonPropertyName("disk")]
        public string Disk { get; set; } = "local";
        [JsonPropertyName("filename")]
        public string? Filename { get; set; }
        [JsonPropertyName("pages")]
        public uint Pages { get; set; } = 0;
        [JsonPropertyName("created_at")]
        public ulong? CreatedAt { get; set; }
        [JsonPropertyName("updated_at")]
        public ulong? UpdatedAt { get; set; }
        [JsonPropertyName("error")]
        public ErrorResponse? Error { get; set; }
    }
}
