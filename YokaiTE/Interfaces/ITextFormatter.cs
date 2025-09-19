namespace YokaiTE.Interfaces;

public interface ITextFormatter
{
    Task ApplyBoldAsync();
    Task ApplyItalicAsync();
    Task ApplyUnderline();
    Task ApplyStrikethrough();
}