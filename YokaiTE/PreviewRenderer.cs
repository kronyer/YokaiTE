using SkiaSharp;
using System.Text.RegularExpressions;
using YokaiTE;

public static class PreviewRenderer
{
    // Mesmo tamanho da .card-preview
    private const int Height = 274; // 274.399 -> arredondei
    private static readonly int Width = (int)Math.Round(Height * (111.0/157.0)); // ≈ 194
    private const int Radius = 4;
    private const int PadX = 12, PadY = 12;
    private const string BgHex = "#F2F2F2";
    private const string TextHex = "#181818";
    private const float FontSize = 7f;
    private const float LineHeight = 1.35f * FontSize; // ~18.9px

    public static string RenderPngBase64(Document doc)
    {
        var html = doc.Content;
        // 1) Extrai parágrafos: cada <div> é um parágrafo, remove demais tags
        var paragraphs = ExtractParagraphs(html);

        using var surface = SKSurface.Create(new SKImageInfo(Width, Height));
        var canvas = surface.Canvas;
        canvas.Clear(SKColors.Transparent);

        // Fundo com cantos arredondados
        using (var bgPaint = new SKPaint { Color = ParseColor(BgHex), IsAntialias = true })
        {
            var rrect = new SKRoundRect(new SKRect(0, 0, Width, Height), Radius, Radius);
            canvas.DrawRoundRect(rrect, bgPaint);
        }

        // Texto
        using var textPaint = new SKPaint
        {
            Color = ParseColor(TextHex),
            IsAntialias = true,
            TextSize = FontSize,
            Typeface = SKTypeface.FromFamilyName("Cabinet Grotesk", SKFontStyle.Normal) // ajuste se quiser outra
        };

        float x = PadX;
        float y = PadY; // top
        float maxW = Math.Max(0, Width - PadX * 2);
        float maxH = Math.Max(0, Height - PadY * 2);
        int maxLines = (int)Math.Floor(maxH / LineHeight);

        var lines = new List<string>();
        for (int i = 0; i < paragraphs.Count; i++)
        {
            WrapInto(paragraphs[i], maxW, textPaint, lines);
            if (i < paragraphs.Count - 1) lines.Add(""); // linha em branco entre <div>s
        }

        // Corta com reticências se passar
        if (lines.Count > maxLines)
        {
            lines = lines.Take(maxLines).ToList();
            if (lines.Count > 0)
            {
                var last = lines[^1];
                // garante que "…" caiba
                while (last.Length > 0 && textPaint.MeasureText(last + "…") > maxW)
                    last = last[..^1];
                lines[^1] = last + "…";
            }
        }

        // Desenha
        foreach (var line in lines)
        {
            // baseline top + ascender: use DrawText com y atual
            canvas.DrawText(line, x, y, textPaint);
            y += LineHeight;
        }

        using var img = surface.Snapshot();
        using var data = img.Encode(SKEncodedImageFormat.Png, 80);
        return Convert.ToBase64String(data.ToArray());
    }

    private static List<string> ExtractParagraphs(string html)
    {
        var list = new List<string>();
        if (string.IsNullOrWhiteSpace(html)) return list;

        // Pega conteúdo de <div> (como parágrafo)
        var divMatches = Regex.Matches(html, @"<div\b[^>]*>(.*?)</div>", RegexOptions.Singleline | RegexOptions.IgnoreCase);
        if (divMatches.Count > 0)
        {
            foreach (Match m in divMatches)
            {
                var inner = m.Groups[1].Value;
                list.Add(StripTags(inner));
            }
        }
        else
        {
            list.Add(StripTags(html));
        }

        // normaliza espaços
        for (int i = 0; i < list.Count; i++)
            list[i] = Regex.Replace(System.Net.WebUtility.HtmlDecode(list[i]), @"\s+", " ").Trim();

        return list.Where(s => s.Length > 0).ToList();
    }

    private static string StripTags(string html)
    {
        // remove todas as tags
        return Regex.Replace(html ?? "", "<.*?>", "");
    }

    private static void WrapInto(string text, float maxW, SKPaint paint, List<string> acc)
    {
        var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (words.Length == 0) { acc.Add(""); return; }

        var line = "";
        foreach (var w in words)
        {
            var test = string.IsNullOrEmpty(line) ? w : line + " " + w;
            if (paint.MeasureText(test) <= maxW)
            {
                line = test;
            }
            else
            {
                if (!string.IsNullOrEmpty(line)) acc.Add(line);

                // palavra maior que largura -> quebra “no meio”
                if (paint.MeasureText(w) > maxW)
                {
                    var piece = "";
                    foreach (var ch in w)
                    {
                        var t2 = piece + ch;
                        if (paint.MeasureText(t2) <= maxW) piece = t2;
                        else { acc.Add(piece); piece = ch.ToString(); }
                    }
                    line = piece;
                }
                else
                {
                    line = w;
                }
            }
        }
        if (!string.IsNullOrEmpty(line)) acc.Add(line);
    }

    private static SKColor ParseColor(string hex)
    {
        return SKColor.TryParse(hex, out var c) ? c : SKColors.Black;
    }
}
