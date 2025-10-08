namespace YokaiTE.Interfaces;

public interface IDocumentService
{
    Task<YokaiTE.Document?> LoadAsync(Guid id);
    Task SaveAsync(Document doc);
    Task UpdateLastOpenedAsync(Document doc);
    Task ExportAsync(Document doc);
    Task<List<DocumentMetadata>> GetAllMetadataAsync();
    Task DeleteAsync(Guid id);
    Task SetFavorite(Guid id);
}