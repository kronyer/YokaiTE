namespace YokaiTE.Utils;

public sealed class SelectionState
{
    public bool Bold { get; set; }
    public bool Italic { get; set; }
    public bool Underline { get; set; }
    public bool Strike { get; set; }
    public bool IsMixed { get; set; }
    public string TextAlign { get; set; } = "left";
    public string FontSize { get; set; } = "16px";
}