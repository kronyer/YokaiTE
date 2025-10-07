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
                await UpdateMetadata(doc);

            }
            return doc;
        }

        public async Task SaveAsync(YokaiTE.Document doc)
        {
            var isNew = doc.Id == 0;
            if (isNew && doc.CreatedAt == default) doc.CreatedAt = DateTime.Now;
            doc.LastModified = DateTime.Now;

            doc.PreviewPngBase64 = PreviewRenderer.RenderPngBase64(doc);

            if (isNew)
            {
                // adiciona como novo registro
                var addRecord = new StoreRecord<YokaiTE.Document> { Storename = "documents", Data = doc };
                await _db.AddRecord(addRecord);

                // tenta recuperar o id gerado pelo IndexedDB
                var all = await _db.GetRecords<YokaiTE.Document>("documents");
                var stored = all
                    .OrderByDescending(d => d.LastModified)
                    .FirstOrDefault(d => d.Title == doc.Title && d.LastModified == doc.LastModified);

                if (stored != null)
                {
                    doc.Id = stored.Id;
                }
                // atualiza metadados com o id correto (se encontrado)
                await UpdateMetadata(doc);
            }
            else
            {
                var record = new StoreRecord<YokaiTE.Document> { Storename = "documents", Data = doc };
                await _db.UpdateRecord(record);
                await UpdateMetadata(doc);
            }
        }
        
        public async Task<List<DocumentMetadata>> GetAllMetadataAsync()
        {
            return await _db.GetRecords<DocumentMetadata>("documentMetadata");
        }
        
public async Task DeleteAsync(long id)
        {
            // Deleta o documento principal
            await _db.DeleteRecord<long>("documents", id);
            
            // Deleta os metadados correspondentes
            await _db.DeleteRecord<long>("documentMetadata", id);
        }
        
        
        private async Task UpdateMetadata(YokaiTE.Document doc)
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

        public async Task UpdateLastOpenedAsync(YokaiTE.Document doc)
        {
            doc.LastOpened = DateTime.Now;
            var record = new StoreRecord<YokaiTE.Document> { Storename = "documents", Data = doc };
            await _db.UpdateRecord(record);
            await UpdateMetadata(doc);
        }

        public Task ExportAsync(YokaiTE.Document doc)
        {
            return _fileService.ExportAsync(doc);
        }

        public async Task SetFavorite(long id)
        {
            var doc = await _db.GetRecordById<long, YokaiTE.Document>("documents", id);
            if (doc == null) return;

            doc.Favorite = !doc.Favorite;

            var record = new StoreRecord<YokaiTE.Document> { Storename = "documents", Data = doc };
            await _db.UpdateRecord(record);

            await UpdateMetadata(doc);
        }
    }
}