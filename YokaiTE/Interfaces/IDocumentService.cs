namespace YokaiTE.Interfaces;

public interface IDocumentService
{
    Task<YokaiTE.Document?> LoadAsync(long id);
    Task SaveAsync(YokaiTE.Document doc);
    Task UpdateLastOpenedAsync(YokaiTE.Document doc);
    Task ExportAsync(YokaiTE.Document doc);
    Task<List<DocumentMetadata>> GetAllMetadataAsync();
    Task DeleteAsync(long id);
    Task SetFavorite(long id);
}