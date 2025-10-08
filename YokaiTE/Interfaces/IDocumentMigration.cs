using YokaiTE;

namespace YokaiTE.Utils;

public interface IDocumentMigration
{
    string FromVersion { get; }
    string ToVersion { get; }

    void Apply(Document doc);
}