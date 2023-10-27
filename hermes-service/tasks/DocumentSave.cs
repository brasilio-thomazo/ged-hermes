using br.dev.optimus.hermes.lib.models;
using br.dev.optimus.hermes.service.interfaces;
using log4net;
using System.Text.Json;

namespace br.dev.optimus.hermes.service.tasks
{
    internal class DocumentSave : ITask
    {
        public string Filename { get; set; } = string.Empty;
        private readonly List<Document> documents;
        private readonly ILog log = LogManager.GetLogger(typeof(DocumentSave));

        public DocumentSave(ref List<Document> documents)
        {
            this.documents = documents;
        }

        public async Task RunAsync()
        {
            Console.WriteLine($"document save {Filename}");
            if (Filename == string.Empty) return;
            try
            {
                var json = JsonSerializer.Serialize(documents);
                var file = new FileInfo(Filename);
                var tmp = $"{file.DirectoryName}/data-new.json";
                Console.WriteLine($"saving {tmp}");
                await File.WriteAllTextAsync(tmp, json);
                Console.WriteLine($"saved {tmp}");
                File.Delete(Filename);
                File.Move(tmp, Filename);
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }
    }
}
