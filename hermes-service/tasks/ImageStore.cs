using br.dev.optimus.hermes.lib;
using br.dev.optimus.hermes.lib.models;
using br.dev.optimus.hermes.lib.responses;
using br.dev.optimus.hermes.service.interfaces;
using br.dev.optimus.hermes.service.models;
using Grpc.Core;
using log4net;

namespace br.dev.optimus.hermes.service.tasks
{
    class ImageStore : ITask
    {
        private readonly List<Document> documents;
        private readonly int index;
        private readonly Client client;
        private readonly QueueStatus status;
        private readonly ILog log = LogManager.GetLogger(typeof(ImageStore));

        public ImageStore(ref List<Document> documents, int index, Client client, ref QueueStatus status)
        {
            this.documents = documents;
            this.index = index;
            this.client = client;
            this.status = status;
        }

        public async Task RunAsync()
        {
            Console.WriteLine($"[{index}] document.image::store [{documents[index].Code}] [{documents[index].Id}]");
            status.PlusRunning();
            if (!documents[index].IsReday)
            {
                status.MinusRunning();
                status.Completed++;
                return;
            }
            if (documents[index].Id == null)
            {
                status.MinusRunning();
                status.Completed++;
                return;
            }
            try
            {
                documents[index] = await client.StoreImageAsync(documents[index]);
                status.Completed++;
                status.MinusRunning();
                return;
            }
            catch (RpcException ex)
            {
                log.Error(ex);
                documents[index].Image.Error = new ErrorResponse
                {
                    Message = ex.Message,
                    Code = ex.StatusCode,
                };
            }
            catch (Exception ex)
            {
                log.Error(ex);
                documents[index].Image.Error = new ErrorResponse
                {
                    Message = ex.Message,
                };
            }
            status.Completed++;
            status.MinusRunning();
            status.Errors++;
        }
    }
}
