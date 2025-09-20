using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using YokaiTE.Interfaces;

namespace YokaiTE.Utils.FileHandlers
{
    public class DocumentMetrics : IDocumentMetrics
    {
        private static readonly Regex _htmlStrip = new Regex("<.*?>", RegexOptions.Compiled);

        public int CharCount(Document doc)
        {
            if (doc == null || string.IsNullOrEmpty(doc.Content))
                return 0;

            var decoded = WebUtility.HtmlDecode(_htmlStrip.Replace(doc.Content, string.Empty));
            return decoded.Length;
        }

        public int WordCount(Document doc)
        {
            if (doc == null || string.IsNullOrWhiteSpace(doc.Content))
                return 0;

            var decoded = WebUtility.HtmlDecode(_htmlStrip.Replace(doc.Content, string.Empty)).Trim();
            return decoded.Split(' ', System.StringSplitOptions.RemoveEmptyEntries).Length;
        }

        public int GetDocumentSizeInBytes(Document doc)
        {
            if (doc == null)
                return 0;

            var json = JsonSerializer.Serialize(doc);
            return Encoding.UTF8.GetByteCount(json);
        }
    }
}