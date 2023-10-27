using br.dev.optimus.hermes.lib.responses;
using System.Text.Json.Serialization;

namespace br.dev.optimus.hermes.lib.models
{
    public class Document
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }
        [JsonPropertyName("document_type_id")]
        public ulong? DocumentTypeId { get; set; }
        [JsonPropertyName("department_id")]
        public ulong? DepartmentId { get; set; }
        [JsonPropertyName("code")]
        public string? Code { get; set; }
        [JsonPropertyName("identity")]
        public string? Identity { get; set; }
        [JsonPropertyName("name")]
        public string? Name { get; set; }
        [JsonPropertyName("comment")]
        public string? Comment { get; set; }
        [JsonPropertyName("storage")]
        public string? Storage { get; set; }
        [JsonPropertyName("date_document")]
        public string? DateDocument { get; set; }
        [JsonPropertyName("created_at")]
        public ulong? CreatedAt { get; set; }
        [JsonPropertyName("updated_at")]
        public ulong? UpdatedAt { get; set; }
        [JsonPropertyName("is_reday")]
        public bool IsReday { get; set; } = false;
        [JsonPropertyName("files")]
        public IEnumerable<DocumentFile>? Files { get; set; }
        [JsonPropertyName("image")]
        public DocumentImage Image { get; set; } = new();
        [JsonPropertyName("error")]
        public ErrorResponse? Error { get; set; }
    }
}
