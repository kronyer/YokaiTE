namespace YokaiTE;

public class Document
{
    public long Id { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastModified { get; set; }
    public DateTime LastOpened { get; set; }

    public string BackgroundColor { get; set; } = "#F2F2F2";
    //TODO remove IsPAged
    public bool IsPaged { get; set; }
    public bool IsSimple { get; set; }

    //TODO remove language
    public string? Language { get; set; }
    public string? PreviewPngBase64 { get; set; } // preview persistido

}