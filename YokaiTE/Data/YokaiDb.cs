using IndexedDB.Blazor;

namespace YokaiTE.Data;

public class YokaiDb : IndexedDB.Blazor.IndexedDb
{
    public IndexedSet<Document> documents { get; set; }

    public YokaiDb(Microsoft.JSInterop.IJSRuntime js) : base(js, "YokaiTE", 1) { }
}