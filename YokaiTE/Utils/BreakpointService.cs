using System;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace YokaiTE.Utils;
public class BreakpointService : IAsyncDisposable
{
    private readonly IJSRuntime _js;
    private DotNetObjectReference<BreakpointService>? _dotNetRef;
    public string CurrentBreakpoint { get; private set; } = "desktop";

    public event Action<string>? OnChange;

    public BreakpointService(IJSRuntime js)
    {
        _js = js;
    }

    public async Task InitializeAsync()
    {
        if (_dotNetRef != null) return;
        _dotNetRef = DotNetObjectReference.Create(this);
        await _js.InvokeVoidAsync("registerBreakpointListener", _dotNetRef);
        try
        {
            var bp = await _js.InvokeAsync<string>("getCurrentBreakpoint");
            UpdateBreakpointInternal(bp);
        }
        catch { }
    }

    [JSInvokable]
    public Task NotifyBreakpoint(string breakpoint)
    {
        UpdateBreakpointInternal(breakpoint);
        return Task.CompletedTask;
    }

    private void UpdateBreakpointInternal(string bp)
    {
        if (string.IsNullOrEmpty(bp)) return;
        if (bp == CurrentBreakpoint) return;
        CurrentBreakpoint = bp;
        OnChange?.Invoke(bp);
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            await _js.InvokeVoidAsync("unregisterBreakpointListener");
        }
        catch { }
        _dotNetRef?.Dispose();
        _dotNetRef = null;
    }
}