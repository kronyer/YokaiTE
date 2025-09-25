namespace YokaiTE.Interfaces;

public interface IFileInfoProvider
{
    string GetSubtitle(DocumentMetadata doc, string selectedField);
    string GetTimeAgo(DateTime dateTime);
}