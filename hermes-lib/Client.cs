using br.dev.optimus.hermes.grpc;
using br.dev.optimus.hermes.lib.errors;
using br.dev.optimus.hermes.lib.models;
using Google.Protobuf;
using Grpc.Core;
using iText.IO.Image;
using iText.Kernel.Pdf;
using iText.Layout.Element;

namespace br.dev.optimus.hermes.lib
{
    public class Client
    {
        public string Host { get; set; } = "localhost";
        public int Port { get; set; } = 50051;
        public string Username { get; set; } = "system";
        public string Password { get; set; } = "system";

        private Hermes.HermesClient? client;
        private readonly ListRequest request = new();

        private Hermes.HermesClient GetClient()
        {
            var channel = new Channel(Host, Port, ChannelCredentials.Insecure);
            return new Hermes.HermesClient(channel);
        }

        public IEnumerable<Department>? GetDepartments()
        {
            client ??= GetClient();
            var reply = client.DepartmentList(request);
            if (reply == null) return null;

            return reply.Data.Select(item => new Department
            {
                Id = item.Id,
                Name = item.Name,
                CreatedAt = item.CreatedAt,
                UpdatedAt = item.UpdatedAt,
            });
        }

        public async Task<IEnumerable<Department>?> GetDepartmentsAsync()
        {
            client ??= GetClient();
            var reply = await client.DepartmentListAsync(request);
            if (reply == null) return null;
            return reply.Data.Select(item => new Department
            {
                Id = item.Id,
                Name = item.Name,
                CreatedAt = item.CreatedAt,
                UpdatedAt = item.UpdatedAt,
            });
        }

        public IEnumerable<DocumentType>? GetDocumentTypes()
        {
            client ??= GetClient();
            var reply = client.DocumentTypeList(request);
            if (reply == null) return null;

            return reply.Data.Select(item => new DocumentType
            {
                Id = item.Id,
                Name = item.Name,
                CreatedAt = item.CreatedAt,
                UpdatedAt = item.UpdatedAt,
            });
        }

        public async Task<IEnumerable<DocumentType>?> GetDocumentTypesAsync()
        {
            client ??= GetClient();
            var reply = await client.DocumentTypeListAsync(request);
            if (reply == null) return null;

            return reply.Data.Select(item => new DocumentType
            {
                Id = item.Id,
                Name = item.Name,
                CreatedAt = item.CreatedAt,
                UpdatedAt = item.UpdatedAt,
            });
        }

        public async Task<Document> StoreDocumentAsync(Document document)
        {
            if (!document.IsReday) throw new NotRedayException("document is not reday");
            client ??= GetClient();
            if (document.DepartmentId == null) throw new Exception("department is null");
            if (document.DocumentTypeId == null) throw new Exception("document type is null");
            if (document.Name == null) throw new Exception("name is null");
            if (document.Identity == null) throw new Exception("identity is null");
            if (document.Code == null) throw new Exception("code is null");
            if (document.DateDocument == null) throw new Exception("date document is null");

            var req = new DocumentRequest
            {
                DepartmentId = (ulong)document.DepartmentId,
                DocumentTypeId = (ulong)document.DocumentTypeId,
                Name = document.Name,
                Identity = document.Identity,
                Code = document.Code,
                DateDocument = document.DateDocument,
            };
            if (document.Comment != null) req.Comment = document.Comment;
            if (document.Storage != null) req.Storage = document.Storage;

            var reply = await client.DocumentStoreAsync(req);
            document.Id = reply.Id;
            document.CreatedAt = reply.CreatedAt;
            document.UpdatedAt = reply.UpdatedAt;
            document.Error = null;
            return document;
        }

        public async Task<Document> StoreImageAsync(Document document)
        {
            client ??= GetClient();
            if (document.Id == null) throw new Exception("document id is null");
            GeneratePDF(ref document);
            if (document.Image.Filename == null) throw new Exception("document image is null");
            var req = new DocumentImageRequest
            {
                Info = new DocumentImageInfo
                {
                    DocumentId = document.Image.DocumentId,
                    Disk = document.Image.Disk,
                    Extension = ".pdf",
                    Pages = document.Image.Pages,
                }
            };

            var store = client.DocumentImageStore();
            var writer = store.RequestStream;
            await writer.WriteAsync(req);
            using var file = File.OpenRead(document.Image.Filename);
            byte[] buffer = new byte[1024 * 1024];
            int read;
            while ((read = file.Read(buffer, 0, buffer.Length)) > 0)
            {
                await writer.WriteAsync(new DocumentImageRequest { Content = ByteString.CopyFrom(buffer, 0, read) });
            }
            await writer.CompleteAsync();
            var reply = await store.ResponseAsync;
            document.Image.Id = reply.Id;
            document.Image.CreatedAt = reply.CreatedAt;
            document.Image.UpdatedAt = reply.UpdatedAt;
            document.Error = null;
            document.Image.Error = null;
            return document;
        }

        private static void GeneratePDF(ref Document document)
        {
            if (document.Id == null) throw new Exception("document id is null");
            if (document.Files == null) throw new Exception("document files is null");
            var tmp = Path.GetTempPath();
            var filename = Path.Combine(tmp, document.Id);
            using var writer = new PdfWriter(filename);
            using var pdf = new PdfDocument(writer);
            pdf.AddNewPage();
            var doc = new iText.Layout.Document(pdf);
            var page = 1;
            var pages = document.Files.Count();
            var ordered = document.Files.OrderBy(item => item.Page);
            foreach (var file in ordered)
            {
                var image = new Image(ImageDataFactory.Create(@$"{file.Path}\{file.Name}"))
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);
                doc.Add(image);
                page++;
                if (page < pages) pdf.AddNewPage();
            }
            doc.Close();
            document.Image.DocumentId = document.Id;
            document.Image.Filename = filename;
            document.Image.Pages = (uint)pages;
        }
    }
}