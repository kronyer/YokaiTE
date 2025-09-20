namespace YokaiTE.Interfaces
{
    public interface IDocumentMetrics
    {
        int CharCount(Document doc);
        int WordCount(Document doc);
        int GetDocumentSizeInBytes(Document doc);
    }
}