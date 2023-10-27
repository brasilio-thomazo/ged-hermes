using br.dev.optimus.hermes.app.interfaces;
using br.dev.optimus.hermes.lib;
using br.dev.optimus.hermes.lib.models;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace br.dev.optimus.hermes.app.tasks
{
    class ImageStore : ITask
    {
        private readonly List<Document> documents;
        private readonly int index;
        private readonly Client client;
        public EventHandler<List<Document>>? UpdatedDocuments;
        public EventHandler<string>? UpdatedLog;

        public ImageStore(ref List<Document> documents, int index, Client client)
        {
            this.documents = documents;
            this.index = index;
            this.client = client;
        }

        public async Task RunAsync()
        {
            try
            {
                UpdateLog($"[{index}][{documents[index].Id}] storing document image");
                documents[index] = await client.StoreImageAsync(documents[index]);
                UpdateLog($"[{index}][{documents[index].Id}]document image store complete with id {documents[index].Image.Id}");
            }
            catch (RpcException ex)
            {
                UpdateLog($"[{index}][{documents[index].Id}] {ex.Message}");
                documents[index].Image.Error = new lib.responses.ErrorResponse
                {
                    Message = ex.Message,
                    Code = ex.StatusCode
                };
            }
        }

        protected virtual void UpdateLog(string message)
        {
            UpdatedLog?.Invoke(this, message);
        }

        protected virtual void UpdateDocuments(List<Document> documents)
        {
            UpdatedDocuments?.Invoke(this, documents);
        }
    }
}
