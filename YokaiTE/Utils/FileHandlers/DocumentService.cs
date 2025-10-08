using TG.Blazor.IndexedDB;
using YokaiTE.Interfaces;

namespace YokaiTE.Utils.FileHandlers
{
    public class DocumentService : IDocumentService
    {
        private readonly IndexedDBManager _db;
        private readonly IFileService _fileService;

        public DocumentService(IndexedDBManager db, IFileService fileService)
        {
            _db = db;
            _fileService = fileService;
        }

        public async Task<YokaiTE.Document?> LoadAsync(Guid id)
        {
            var doc = await _db.GetRecordById<Guid, Document>("documents", id);
            if (doc != null)
            {
                doc.LastOpened = DateTime.Now;
                var record = new StoreRecord<Document> { Storename = "documents", Data = doc };
                await _db.UpdateRecord(record);
                await UpdateMetadata(doc);

            }
            return doc;
        }

        public async Task SaveAsync(Document doc)
        {
            var isNew = doc.Id == Guid.Empty;
            if (isNew)
            {
                doc.Id = Guid.NewGuid();
                if (doc.CreatedAt == default) doc.CreatedAt = DateTime.Now;
            }
            doc.LastModified = DateTime.Now;

            doc.PreviewPngBase64 = PreviewRenderer.RenderPngBase64(doc);

            if (isNew)
            {
                var addRecord = new StoreRecord<Document> { Storename = "documents", Data = doc };
                await _db.AddRecord(addRecord);

                var all = await _db.GetRecords<Document>("documents");
                var stored = all
                    .OrderByDescending(d => d.LastModified)
                    .FirstOrDefault(d => d.Title == doc.Title && d.LastModified == doc.LastModified);

                if (stored != null)
                {
                    doc.Id = stored.Id;
                }
                await UpdateMetadata(doc);
            }
            else
            {
                var record = new StoreRecord<Document> { Storename = "documents", Data = doc };
                await _db.UpdateRecord(record);
                await UpdateMetadata(doc);
            }
        }

        public async Task<List<DocumentMetadata>> GetAllMetadataAsync()
        {
            return await _db.GetRecords<DocumentMetadata>("documentMetadata");
        }

        public async Task DeleteAsync(Guid id)
        {
            // Deleta o documento principal
            await _db.DeleteRecord<Guid>("documents", id);

            // Deleta os metadados correspondentes
            await _db.DeleteRecord<Guid>("documentMetadata", id);
        }


        private async Task UpdateMetadata(Document doc)
        {
            var metadata = new DocumentMetadata
            {
                Id = doc.Id,
                Title = doc.Title,
                CreatedAt = doc.CreatedAt,
                LastModified = doc.LastModified,
                LastOpened = doc.LastOpened,
                BackgroundColor = doc.BackgroundColor,
                PreviewPngBase64 = doc.PreviewPngBase64,
                Favorite = doc.Favorite,
            };

            var metadataRecord = new StoreRecord<DocumentMetadata> { Storename = "documentMetadata", Data = metadata };
            await _db.UpdateRecord(metadataRecord);
        }

        public async Task UpdateLastOpenedAsync(Document doc)
        {
            doc.LastOpened = DateTime.Now;
            var record = new StoreRecord<Document> { Storename = "documents", Data = doc };
            await _db.UpdateRecord(record);
            await UpdateMetadata(doc);
        }

        public Task ExportAsync(Document doc)
        {
            return _fileService.ExportAsync(doc);
        }

        public async Task SetFavorite(Guid id)
        {
            var doc = await _db.GetRecordById<Guid, Document>("documents", id);
            if (doc == null) return;

            doc.Favorite = !doc.Favorite;

            var record = new StoreRecord<Document> { Storename = "documents", Data = doc };
            await _db.UpdateRecord(record);

            await UpdateMetadata(doc);
        }
    }
}