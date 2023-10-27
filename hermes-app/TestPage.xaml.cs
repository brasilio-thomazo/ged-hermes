using br.dev.optimus.hermes.app.binds;
using br.dev.optimus.hermes.lib;
using br.dev.optimus.hermes.lib.models;
using log4net;
using Microsoft.Extensions.Configuration;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace br.dev.optimus.hermes.app
{
    public partial class TestPage : Page
    {
        private readonly Client client;
        private readonly List<string> filenames = new();
        private readonly Random random = new();
        private readonly ILog log = LogManager.GetLogger(typeof(TestPage));
        private readonly List<Document> documents = new();

        public ObservableCollection<DepartmentBind> Departments { get; set; } = new ObservableCollection<DepartmentBind>();
        public ObservableCollection<DocumentTypeBind> DocumentTypes { get; set; } = new ObservableCollection<DocumentTypeBind>();
        public IConfiguration Configuration { get; set; } = new ConfigurationBuilder().AddEnvironmentVariables().Build();

        public TestPage()
        {
            InitializeComponent();
            var host = Configuration.GetValue("GRPC_HOST", "localhost");
            var port = Configuration.GetValue("GRPC_PORT", 50051);
            client = new Client { Host = host ??= "localhost", Port = port };
            _ = LoadDepartmentsAsync();
            _ = LoadDocumentTypesAsync();
        }

        private async Task LoadDepartmentsAsync()
        {
            try
            {
                Departments.Add(new DepartmentBind { Data = new Department { Name = "Aguarde...", Id = 0 } });
                documentTypes.SelectedIndex = 0;
                var reply = await client.GetDepartmentsAsync();
                Departments.Clear();
                if (reply == null) return;
                foreach (var item in reply)
                {
                    Departments.Add(new DepartmentBind { Data = item });
                }
                departments.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        private async Task LoadDocumentTypesAsync()
        {
            DocumentTypes.Add(new DocumentTypeBind { Data = new DocumentType { Name = "Aguarde...", Id = 0 } });
            documentTypes.SelectedIndex = 0;
            try
            {
                var reply = await client.GetDocumentTypesAsync();
                DocumentTypes.Clear();
                if (reply == null) return;
                foreach (var item in reply)
                {
                    DocumentTypes.Add(new DocumentTypeBind { Data = item });
                }
                documentTypes.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        private async void DepartmentReload(object sender, RoutedEventArgs e)
        {
            Departments.Clear();
            await LoadDepartmentsAsync();
        }

        private async void DocumentTypeReload(object sender, RoutedEventArgs e)
        {
            DocumentTypes.Clear();
            await LoadDocumentTypesAsync();
        }

        private void SelectImages(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog()
            {
                Filter = "Images (.jpg;.png;.tiff)|*.jpg;*.png;*.tiff",
                Multiselect = true,

            };

            if (dialog.ShowDialog() == true)
            {
                filenames.Clear();
                filenames.AddRange(dialog.FileNames);
                images.Text = string.Join(";", dialog.FileNames);
            }
        }

        private void GenerateFile(object sender, RoutedEventArgs e)
        {
            if (departments.SelectedValue == null)
            {
                MessageBox.Show("Selecione um departamento.");
                return;
            }

            if (documentTypes.SelectedValue == null)
            {
                MessageBox.Show("Selecione um tipo de documento.");
                return;
            }

            if (filenames.Count == 0)
            {
                MessageBox.Show("Selecione ao menos uma imagem");
                return;
            }

            documents.Clear();

            for (int i = 0; i < random.Next(10, 50); i++)
            {
                documents.Add(GenerateDocument());
            }

            var options = new JsonSerializerOptions { AllowTrailingCommas = true, WriteIndented = true };
            var json = JsonSerializer.Serialize(documents, options);
            logs.Text = json;
        }

        private async void Send(object sender, RoutedEventArgs e)
        {
            logs.Text = "";
            foreach (var document in documents)
            {
                try
                {
                    await client.StoreDocumentAsync(document);
                    logs.Text += $"document::store [{document.Id}]\r\n";
                    await client.StoreImageAsync(document);
                    logs.Text += $"document.image::store [{document.Image?.Filename}]\r\n";
                }
                catch (Exception ex)
                {
                    log.Error(ex);
                    MessageBox.Show($"Error: {ex.Message}");
                    break;
                }

            }
        }

        private static string GenerateName()
        {
            var rng = new Random();
            var names = new string[] {
                "Anakin Skywalker",
                "Kylo Ren",
                "Obi-Wan Kenobi",
                "Luke Skywalker",
                "Leia Organa",
                "Han Solo",
                "Padmé Amidala",
                "Qui-Gon Jinn",
                "Boba Fett",
                "Lando Calrissian"
            };

            return names[rng.Next(0, names.Length - 1)];
        }

        private static string GenerateIdentity()
        {
            var rng = new Random();
            var data = "0123456789";
            var builder = new StringBuilder();
            for (var i = 0; i < 11; i++)
            {
                builder.Append(data[rng.Next(data.Length)]);
            }
            return builder.ToString();
        }

        private static string GenerateCode()
        {
            var rng = new Random();
            var data = "ABCDEFGHIJKLMNOPQRSWXYZ0123456789";
            var builder = new StringBuilder();
            for (var i = 0; i < 10; i++)
            {
                builder.Append(data[rng.Next(data.Length)]);
            }
            return builder.ToString();
        }

        private Document GenerateDocument()
        {
            var rng = new Random();
            var documentFiles = new List<DocumentFile>();
            for (int i = 0; i < rng.Next(1, 20); i++)
            {
                var info = new FileInfo(filenames[rng.Next(filenames.Count - 1)]);
                documentFiles.Add(new DocumentFile
                {
                    Path = info.DirectoryName,
                    Name = info.Name,
                    Page = (uint)i,
                });
            }

            return new Document
            {
                DepartmentId = (ulong)departments.SelectedValue,
                DocumentTypeId = (ulong)documentTypes.SelectedValue,
                Identity = GenerateIdentity(),
                Code = GenerateCode(),
                Name = GenerateName(),
                Files = documentFiles,
                DateDocument = DateTime.Now.AddDays(rng.Next(-60, 0)).ToString("yyyy-MM-dd"),
                IsReday = true,
            };
        }
    }
}
