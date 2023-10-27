using br.dev.optimus.hermes.lib;
using br.dev.optimus.hermes.lib.models;
using br.dev.optimus.hermes.service.interfaces;
using br.dev.optimus.hermes.service.models;
using br.dev.optimus.hermes.service.tasks;
using Grpc.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace br.dev.optimus.hermes.service
{
    internal class App : BackgroundService
    {
        private readonly IConfiguration configuration;
        private readonly ILogger<App> logger;

        private Queue<ITask> queue = new();
        private readonly Stack<DocumentSave> stack = new();
        private readonly List<string> files = new();
        private QueueStatus status = new();
        private readonly Client client;
        private readonly DirectoryInfo directory;
        public App(ILogger<App> logger, IConfiguration configuration)
        {
            this.logger = logger;
            this.configuration = configuration;
            var dir = configuration["WATCH_DIRECTORY"] ??= "./";
            directory = new DirectoryInfo(dir);
            if (!directory.Exists) directory.Create();
            client = new Client
            {
                Host = configuration["GRPC_HOST"] ??= "localhost",
                Port = int.Parse(configuration["GRPC_PORT"] ??= "50051")
            };
        }

        protected override async Task ExecuteAsync(CancellationToken token)
        {
            var maxThread = Environment.ProcessorCount / 2;
            var options = new ParallelOptions { MaxDegreeOfParallelism = maxThread };
            var watcher = new FileSystemWatcher(directory.FullName)
            {
                EnableRaisingEvents = true,
                IncludeSubdirectories = true,
                Filter = "data.json"
            };

            watcher.Created += OnCreated;
            watcher.Changed += OnChanged;

            logger.LogInformation("watching directory {}", directory.FullName);

            while (!token.IsCancellationRequested)
            {
                if (queue.Count > 0)
                {
                    var actions = new List<ITask>();
                    while (queue.Count != 0)
                    {
                        var task = queue.Dequeue();
                        if (task == null) continue;
                        status.Total++;
                        actions.Add(task);
                    }
                    await Parallel.ForEachAsync(actions, options, async (action, cancelToken) => await action.RunAsync());
                    continue;
                }
                if (stack.Count > 0 && status.Total != status.Completed)
                {
                    var actions = new List<DocumentSave>();
                    while (stack.Count != 0)
                    {
                        var task = stack.Pop();
                        if (task == null) continue;
                        files.Remove(task.Filename);
                        status.Total++;
                        actions.Add(task);
                    }
                    await Parallel.ForEachAsync(actions, options, async (action, cancelToken) => await action.RunAsync());
                    continue;
                }
                logger.LogInformation("queue:{} stack:{} running:{} completed:{} errors:{}", queue.Count, stack.Count, status.Running, status.Completed, status.Errors);
                await Task.Delay(5000, token);
            }
        }

        private bool DocumentIsValid(Document document)
        {
            if (!document.IsReday)
            {
                logger.LogInformation("not reday");
                return false;
            }

            if (document.Error != null && document.Error.Code != StatusCode.Unavailable)
            {
                logger.LogInformation("document with error");
                return false;
            }

            return true;
        }

        private bool DocumentImageIsValid(Document document)
        {
            if (document.Image.Id != null)
            {
                logger.LogInformation("document is complete");
                return false;
            }
            if (document.Image.Error != null && document.Image.Error.Code != StatusCode.Unavailable)
            {
                logger.LogInformation("image with error");
                return false;
            }
            return document.Id != null;
        }

        /// <summary>
        /// Event of updated file *data.json
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            if (files.Contains(e.FullPath)) { logger.LogInformation($"file [{e.FullPath}] in queue"); return; }
            try
            {
                var json = File.ReadAllText(e.FullPath);
                var documents = JsonSerializer.Deserialize<List<Document>>(json);
                var isQueue = false;
                logger.LogInformation($"file [{e.FullPath}] changed");
                if (documents == null || documents.Count == 0) return;
                for (var i = 0; i < documents.Count; i++)
                {
                    var document = documents[i];
                    if (!DocumentIsValid(document)) continue;
                    if (DocumentImageIsValid(document))
                    {
                        queue.Enqueue(new ImageStore(ref documents, i, client, ref status));
                        isQueue = true;
                        continue;
                    }
                    queue.Enqueue(new DocumentStore(ref documents, i, client, ref queue, ref status));
                    isQueue = true;
                }
                if (!isQueue) return;
                files.Add(e.FullPath);
                stack.Push(new DocumentSave(ref documents) { Filename = e.FullPath });
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
            }
        }

        /// <summary>
        /// Event of created file *data.json
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCreated(object sender, FileSystemEventArgs e)
        {
            if (files.Contains(e.FullPath)) { logger.LogInformation($"file [{e.FullPath}] in queue"); return; }
            try
            {
                var json = File.ReadAllText(e.FullPath);
                var documents = JsonSerializer.Deserialize<List<Document>>(json);
                var isQueue = false;
                logger.LogInformation($"file [{e.FullPath}] changed");
                if (documents == null || documents.Count == 0) return;
                for (var i = 0; i < documents.Count; i++)
                {
                    var document = documents[i];
                    if (!DocumentIsValid(document)) continue;
                    if (DocumentImageIsValid(document))
                    {
                        queue.Enqueue(new ImageStore(ref documents, i, client, ref status));
                        isQueue = true;
                        continue;
                    }
                    queue.Enqueue(new DocumentStore(ref documents, i, client, ref queue, ref status));
                    isQueue = true;
                }
                if (!isQueue) return;
                files.Add(e.FullPath);
                stack.Push(new DocumentSave(ref documents) { Filename = e.FullPath });
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
            }
        }
    }
}
