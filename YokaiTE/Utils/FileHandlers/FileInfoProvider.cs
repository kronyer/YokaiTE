using YokaiTE.Interfaces;

namespace YokaiTE.Utils.FileHandlers;

public class FileInfoProvider : IFileInfoProvider
{
    public string GetSubtitle(Document doc, string SelectedField)
    {
        return SelectedField switch
        {
            "Data de edição" => $"Editado por último há {GetTimeAgo(doc.LastModified)}",
            "Data de criação" => $"Criado em {doc.CreatedAt.ToString("dd/MM/yy")}",
            _ => $"Aberto por último há {GetTimeAgo(doc.LastOpened)}"
        };
    }
    
    public string GetTimeAgo(DateTime dateTime)
    {
        var ts = DateTime.Now - dateTime;
        if (ts.TotalMinutes < 1)
            return "agora mesmo";
        if (ts.TotalMinutes < 60)
            return $"{(int)ts.TotalMinutes} minuto{(ts.TotalMinutes < 2 ? "" : "s")}";
        if (ts.TotalHours < 24)
            return $"{(int)ts.TotalHours} hora{(ts.TotalHours < 2 ? "" : "s")}";
        return $"{(int)ts.TotalDays} dia{(ts.TotalDays < 2 ? "" : "s")}";
    }
}