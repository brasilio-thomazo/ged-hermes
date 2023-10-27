using br.dev.optimus.hermes.lib;
using br.dev.optimus.hermes.lib.models;
using br.dev.optimus.hermes.lib.responses;
using br.dev.optimus.hermes.service.interfaces;
using br.dev.optimus.hermes.service.models;
using Grpc.Core;
using log4net;

namespace br.dev.optimus.hermes.service.tasks
{
    internal class DocumentStore : ITask
    {
        private List<Document> documents;
        private readonly Client client;
        private readonly int index;
        private Queue<ITask> queue;
        private QueueStatus status;
        private readonly ILog log = LogManager.GetLogger(typeof(DocumentStore));

        public DocumentStore(ref List<Document> documents, int index, Client client, ref Queue<ITask> queue, ref QueueStatus status)
        {
            this.documents = documents;
            this.index = index;
            this.client = client;
            this.status = status;
            this.queue = queue;
        }

        public async Task RunAsync()
        {
            status.PlusRunning();
            Console.WriteLine($"[{index}] document::store [{documents[index].Code}]");
            if (!documents[index].IsReday)
            {
                status.MinusRunning();
                status.Completed++;
                return;
            }
            try
            {
                documents[index] = await client.StoreDocumentAsync(documents[index]);
                status.Completed++;
                status.MinusRunning();
                var task = new ImageStore(ref documents, index, client, ref status);
                queue.Enqueue(task);
                return;
            }
            catch (RpcException ex)
            {
                documents[index].Error = new ErrorResponse
                {
                    Message = ex.Message,
                    Code = ex.StatusCode,
                };
                log.Error(ex);
            }
            catch (Exception ex)
            {
                documents[index].Error = new ErrorResponse
                {
                    Message = ex.Message,
                };
                log.Error(ex);
            }
            status.Completed++;
            status.MinusRunning();
            status.Errors++;
        }
    }
}
