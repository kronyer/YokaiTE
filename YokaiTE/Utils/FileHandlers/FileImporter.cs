using System.IO.Compression;
using System.Text;
using System.Text.Json;
using Microsoft.JSInterop;
using TG.Blazor.IndexedDB;
using YokaiTE.Interfaces;

namespace YokaiTE.Utils;

public class FileImporter : IFileImporter
{
    private readonly IDocumentService _documentService;

    public FileImporter(IDocumentService documentService)
    {
        _documentService = documentService;
    }
    
    public async Task ImportHaikuDoc(string base64, string filename)
    {
        var bytes = Convert.FromBase64String(base64);

        var header = Encoding.ASCII.GetBytes("YOKAIHAIKU");
        if (bytes.Length < header.Length + 1) return;

        for (int i = 0; i < header.Length; i++)
        {
            if (bytes[i] != header[i]) return;
        }

        var version = bytes[header.Length];

        var compressedLen = bytes.Length - header.Length - 1;
        if (compressedLen <= 0) return;

        var compressed = new byte[compressedLen];
        Buffer.BlockCopy(bytes, header.Length + 1, compressed, 0, compressedLen);

        string json;
        using (var inMs = new MemoryStream(compressed))
        using (var gzip = new GZipStream(inMs, CompressionMode.Decompress))
        using (var reader = new StreamReader(gzip, Encoding.UTF8))
        {
            json = await reader.ReadToEndAsync();
        }

        var imported = JsonSerializer.Deserialize<Document>(json);
        if (imported == null) return;

        // Reset id para criar novo registro
        imported.Id = Guid.Empty;
        imported.CreatedAt = DateTime.Now;
        imported.LastModified = DateTime.Now;
        imported.LastOpened = DateTime.Now;

        // Gerar preview se existir utilitário
        try
        {
            imported.PreviewPngBase64 = PreviewRenderer.RenderPngBase64(imported);
        }
        catch { /* ignore se não disponível */ }

        await _documentService.SaveAsync(imported);
    }

}

public interface IFileImporter
{
    Task ImportHaikuDoc(string base64, string filename);
}

