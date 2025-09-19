using System.IO.Compression;
using System.Text;
using System.Text.Json;
using Microsoft.JSInterop;
using TG.Blazor.IndexedDB;

namespace YokaiTE.Utils;

public class FileImporter : IFileImporter
{
    private IndexedDBManager _dbManager;

    public FileImporter(IndexedDBManager dbManager)
    {
        _dbManager = dbManager;
    }
    
   public async Task ImportHaikuDoc(string base64, string filename)
    {
        var bytes = Convert.FromBase64String(base64);

            var header = Encoding.ASCII.GetBytes("YOKAIHAIKU");
            if (bytes.Length < header.Length + 1) return; //because its an empty file

            for (int i = 0; i < header.Length; i++)
            {
                if (bytes[i] != header[i]) return; //compare if the header is of YOKAIHAIKU
            }

            var version = bytes[header.Length]; // atualmente não usado, queda suave se necessário

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

            // Reset id to allow IndexedDB criar um novo registro
            imported.Id = 0;
            imported.CreatedAt = DateTime.Now;
            imported.LastModified = DateTime.Now;
            imported.LastOpened = DateTime.Now;

            // Opcional: gerar preview se existir utilitário
            try
            {
                imported.PreviewPngBase64 = PreviewRenderer.RenderPngBase64(imported);
            }
            catch { /* ignore se não disponível */ }

            var record = new StoreRecord<Document> { Storename = "documents", Data = imported };
            await _dbManager.AddRecord(record);

            // Recarrega lista e navega para o novo documento (mesma abordagem do SaveDocument)
            var loaded = await _dbManager.GetRecords<Document>("documents");
            var novoDoc = loaded
                .OrderByDescending(d => d.LastModified)
                .FirstOrDefault(d => d.Title == imported.Title && d.LastModified.Date == imported.LastModified.Date && d.LastModified.TimeOfDay.TotalSeconds - imported.LastModified.TimeOfDay.TotalSeconds < 5);
    }

}

public interface IFileImporter
{
    Task ImportHaikuDoc(string base64, string filename);
}

