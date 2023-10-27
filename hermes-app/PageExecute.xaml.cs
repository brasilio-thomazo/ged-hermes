using br.dev.optimus.hermes.app.interfaces;
using br.dev.optimus.hermes.app.tasks;
using br.dev.optimus.hermes.lib;
using br.dev.optimus.hermes.lib.models;
using Grpc.Core;
using log4net;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace br.dev.optimus.hermes.app
{
    /// <summary>
    /// Interação lógica para PageExecute.xam
    /// </summary>
    public partial class PageExecute : Page
    {
        private readonly IConfiguration configuration = new ConfigurationBuilder().AddEnvironmentVariables().Build();
        private readonly ILog log = LogManager.GetLogger(typeof(PageExecute));
        private readonly DirectoryInfo directory;
        private readonly Client client;
        public PageExecute()
        {
            InitializeComponent();
            var dirname = configuration["WATCH_DIRECTORY"] ??= ".";
            var host = configuration["GRPC_HOST"] ??= "localhost";
            var port = configuration["GRPC_PORT"] ??= "50051";
            directory = new DirectoryInfo(dirname);
            if (!directory.Exists) directory.Create();
            client = new Client { Host = host, Port = int.Parse(port) };
        }

        private async void ExecuteFiles(object sender, RoutedEventArgs e)
        {
            logs.Text = "";
            var options = new ParallelOptions
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount / 2
            };
            try
            {
                var matchFiles = Directory.GetFiles(directory.FullName, "data.json", SearchOption.AllDirectories);
                var actions = new List<ITask>();
                foreach (var matchFile in matchFiles)
                {
                    actions.Clear();
                    logs.Text += $"reading {matchFile}... ";
                    var json = await File.ReadAllTextAsync(matchFile);
                    var documents = JsonSerializer.Deserialize<List<Document>>(json);
                    if (documents == null) continue;
                    if (documents.Count == 0) continue;
                    logs.Text += $"there was a total of {documents.Count} documents found.\r\n";
                    for (var i = 0; i < documents.Count; i++)
                    {
                        if (!CheckDocument(documents[i])) continue;
                        var store = new DocumentStore(ref documents, i, client);
                        store.UpdatedLog += UpdateLog;
                        actions.Add(store);
                    }
                    await Parallel.ForEachAsync(actions, options, async (action, token) => await action.RunAsync());
                    actions.Clear();
                    for (var i = 0; i < documents.Count; i++)
                    {
                        if (!CheckImage(documents[i])) continue;
                        var store = new ImageStore(ref documents, i, client);
                        store.UpdatedLog += UpdateLog;
                        actions.Add(store);
                    }
                    await Parallel.ForEachAsync(actions, options, async (action, token) => await action.RunAsync());
                    await SaveFile(matchFile, documents);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"[error]process document {ex.Message}");
                log.Error(ex);
            }
        }

        private void UpdateLog(object? _, string message)
        {
            Dispatcher.Invoke(() =>
            {
                var old = logs.Text;
                logs.Text = message + "\t\n" + old;
            });
        }

        private async Task SaveFile(string filename, List<Document> documents)
        {
            try
            {
                var json = JsonSerializer.Serialize(documents);
                await File.WriteAllTextAsync(filename, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"[error]save file {filename} {ex.Message}");
                log.Error(ex);
            }
        }

        private static bool CheckDocument(Document document)
        {
            if (!document.IsReday)
            {
                return false;
            }

            if (document.Error != null && document.Error.Code != StatusCode.Unavailable)
            {
                return false;
            }

            return document.Id == null;
        }

        private static bool CheckImage(Document document)
        {
            if (!document.IsReday) return false;
            if (document.Error != null && document.Error.Code != StatusCode.Unavailable) return false;
            if (document.Id == null) return false;
            if (document.Image.Id != null) return false;
            if (document.Image.Error != null && document.Image.Error.Code != StatusCode.Unavailable) return false;
            return document.Id != null;
        }
    }
}
