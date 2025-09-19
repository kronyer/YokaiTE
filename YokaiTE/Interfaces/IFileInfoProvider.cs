namespace YokaiTE.Interfaces;

public interface IFileInfoProvider
{
    string GetSubtitle(Document doc, string selectedField);
    string GetTimeAgo(DateTime dateTime);
}