using Grpc.Core;

namespace br.dev.optimus.hermes.lib.responses
{
    public class ErrorResponse
    {
        public StatusCode Code { get; set; } = StatusCode.Unknown;
        public string? Message { get; set; }
    }
}
