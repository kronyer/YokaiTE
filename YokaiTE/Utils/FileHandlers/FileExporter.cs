using System.IO.Compression;
using System.Text;
using System.Text.Json;
using Microsoft.JSInterop;
using TG.Blazor.IndexedDB;
using YokaiTE.Interfaces;

namespace YokaiTE.Utils;

public class FileExporter : IFileExporter
{
    IJSRuntime _JS;
    private readonly IndexedDBManager _db;

    public FileExporter(IJSRuntime JS, IndexedDBManager db)
    {
        _JS = JS;
        _db = db;
    }
    public async Task ExportHaikuDoc(Document document)
    {
        if (document == null) return;

        // Serializa JSON
        var json = JsonSerializer.Serialize(document);

        // Cria um buffer com um header mágico + versão + conteúdo comprimido
        // Header: ASCII "YOKAIHAIKU" (10 bytes) + version byte (1)
        var header = Encoding.ASCII.GetBytes("YOKAIHAIKU");
        var version = new byte[] { 1 };

        byte[] compressed;
        using (var outMs = new MemoryStream())
        {
            // escreve header temporário depois (vamos concatenar)
            using (var gzip = new GZipStream(outMs, CompressionLevel.Optimal, leaveOpen: true))
            using (var writer = new StreamWriter(gzip, Encoding.UTF8))
            {
                writer.Write(json);
            }
            compressed = outMs.ToArray();
        }

        // Junta tudo: header + version + compressed
        byte[] result = new byte[header.Length + version.Length + compressed.Length];
        Buffer.BlockCopy(header, 0, result, 0, header.Length);
        Buffer.BlockCopy(version, 0, result, header.Length, version.Length);
        Buffer.BlockCopy(compressed, 0, result, header.Length + version.Length, compressed.Length);

        // Converte para base64 e chama JS para download
        var base64 = Convert.ToBase64String(result);
        var safeName = string.IsNullOrWhiteSpace(document.Title) ? "document.haiku" : MakeSafeFilename(document.Title) + ".haiku";
        await _JS.InvokeVoidAsync("saveFileFromBase64", safeName, base64, "application/octet-stream");
    }
    
    public async Task ExportHaikuDoc(DocumentMetadata documentMetadata)
    {
        if (documentMetadata == null) return;
        
        var fullDocument = await _db.GetRecordById<Guid, Document>("documents", documentMetadata.Id);
        if (fullDocument == null) return;
        // Serializa JSON
        var json = JsonSerializer.Serialize(fullDocument);

        // Cria um buffer com um header mágico + versão + conteúdo comprimido
        // Header: ASCII "YOKAIHAIKU" (10 bytes) + version byte (1)
        var header = Encoding.ASCII.GetBytes("YOKAIHAIKU");
        var version = new byte[] { 1 };

        byte[] compressed;
        using (var outMs = new MemoryStream())
        {
            // escreve header temporário depois (vamos concatenar)
            using (var gzip = new GZipStream(outMs, CompressionLevel.Optimal, leaveOpen: true))
            using (var writer = new StreamWriter(gzip, Encoding.UTF8))
            {
                writer.Write(json);
            }
            compressed = outMs.ToArray();
        }

        // Junta tudo: header + version + compressed
        byte[] result = new byte[header.Length + version.Length + compressed.Length];
        Buffer.BlockCopy(header, 0, result, 0, header.Length);
        Buffer.BlockCopy(version, 0, result, header.Length, version.Length);
        Buffer.BlockCopy(compressed, 0, result, header.Length + version.Length, compressed.Length);

        // Converte para base64 e chama JS para download
        var base64 = Convert.ToBase64String(result);
        var safeName = string.IsNullOrWhiteSpace(fullDocument.Title) ? "document.haiku" : MakeSafeFilename(fullDocument.Title) + ".haiku";
        await _JS.InvokeVoidAsync("saveFileFromBase64", safeName, base64, "application/octet-stream");
    }
    
    // Helper interno para sanitizar nome de ficheiro
    private static string MakeSafeFilename(string name)
    {
        var invalids = Path.GetInvalidFileNameChars();
        var sb = new StringBuilder();
        foreach (var ch in name)
        {
            if (Array.IndexOf(invalids, ch) >= 0) sb.Append('_'); else sb.Append(ch);
        }
        // trim e limitar comprimento
        var outName = sb.ToString().Trim();
        if (outName.Length > 120) outName = outName.Substring(0, 120);
        return outName;
    }

}

public interface IFileExporter
{
    Task ExportHaikuDoc(Document document);
    Task ExportHaikuDoc(DocumentMetadata document);
}