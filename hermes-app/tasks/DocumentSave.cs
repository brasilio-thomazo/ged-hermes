using br.dev.optimus.hermes.app.interfaces;
using br.dev.optimus.hermes.lib.models;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace br.dev.optimus.hermes.app.tasks
{
    internal class DocumentSave : ITask
    {
        public string? Filename { get; set; }
        private readonly List<Document> documents;
        private readonly ILog log = LogManager.GetLogger(typeof(DocumentSave));

        public DocumentSave(ref List<Document> documents)
        {
            this.documents = documents;
        }

        public async Task RunAsync()
        {
            if (Filename == null) return;
            try
            {
                var json = JsonSerializer.Serialize(documents);
                var tmp = $"{Filename}.new";
                log.Info($"file saving: {tmp}");
                await File.WriteAllTextAsync(tmp, json);
                log.Info($"file saved: {tmp}");
                // File.Delete(Filename);
                // File.Move(tmp, Filename);
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }
    }
}
