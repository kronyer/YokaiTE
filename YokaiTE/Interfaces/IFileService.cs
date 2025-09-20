namespace YokaiTE.Interfaces;

public interface IFileService
{
    Task ExportAsync(YokaiTE.Document doc);
}