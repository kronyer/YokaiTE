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

        public async Task<YokaiTE.Document?> LoadAsync(long id)
        {
            var doc = await _db.GetRecordById<long, YokaiTE.Document>("documents", id);
            if (doc != null)
            {
                doc.LastOpened = DateTime.Now;
                var record = new StoreRecord<YokaiTE.Document> { Storename = "documents", Data = doc };
                await _db.UpdateRecord(record);
            }
            return doc;
        }

        public async Task SaveAsync(YokaiTE.Document doc)
        {
            doc.LastModified = DateTime.Now;
            doc.PreviewPngBase64 = PreviewRenderer.RenderPngBase64(doc);
            var record = new StoreRecord<YokaiTE.Document> { Storename = "documents", Data = doc };
            await _db.UpdateRecord(record);
        }

        public async Task UpdateLastOpenedAsync(YokaiTE.Document doc)
        {
            doc.LastOpened = DateTime.Now;
            var record = new StoreRecord<YokaiTE.Document> { Storename = "documents", Data = doc };
            await _db.UpdateRecord(record);
        }

        public Task ExportAsync(YokaiTE.Document doc)
        {
            return _fileService.ExportAsync(doc);
        }
    }
}