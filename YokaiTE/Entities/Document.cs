namespace YokaiTE;

public class Document
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastModified { get; set; }
    public DateTime LastOpened { get; set; }
    public string BackgroundColor { get; set; } = "#F2F2F2";
    public bool IsSimple { get; set; }
    public bool Favorite { get; set; } = false;
    public string? PreviewPngBase64 { get; set; } // preview persistido

    public string Version { get; set; } = "0.0.0";

}