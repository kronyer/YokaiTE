using YokaiTE.Interfaces;

namespace YokaiTE.Utils.FileHandlers
{
    public class FileService : IFileService
    {
        private readonly IFileExporter _exporter;

        public FileService(IFileExporter exporter)
        {
            _exporter = exporter;
        }

        public Task ExportAsync(YokaiTE.Document doc)
        {
            _exporter.ExportHaikuDoc(doc);
            return Task.CompletedTask;
        }
    }
}
