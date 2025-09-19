using Microsoft.JSInterop;
using YokaiTE.Interfaces;

namespace YokaiTE.Utils.TextHandlers;

public class TextFormatter : ITextFormatter
{
    IJSRuntime _JS;

    public TextFormatter(IJSRuntime JS)
    {
        _JS = JS;
    }
    
    public async Task ApplyBoldAsync()
    {
        await _JS.InvokeVoidAsync("applyBold");
    }
    public async Task ApplyItalicAsync()
    {
        await _JS.InvokeVoidAsync("applyItalic");
    }

    public async Task ApplyUnderline()
    {
        await _JS.InvokeVoidAsync("applyUnderline");
    }

    public async Task ApplyStrikethrough()
    {
        await _JS.InvokeVoidAsync("applyStrikethrough");
    }
    
    
}