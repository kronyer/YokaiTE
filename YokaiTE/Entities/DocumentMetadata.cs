namespace YokaiTE;

public class DocumentMetadata
{
    public long Id { get; set; }
    public string Title { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastModified { get; set; }
    public DateTime LastOpened { get; set; }
    public string BackgroundColor { get; set; } = "#F2F2F2";
    public string? PreviewPngBase64 { get; set; }

}