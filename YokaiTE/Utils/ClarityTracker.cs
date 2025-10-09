using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;

public class ClarityTracker : IDisposable
{
    private readonly NavigationManager _nav;
    private readonly IJSRuntime _js;

    public ClarityTracker(NavigationManager nav, IJSRuntime js)
    {
        _nav = nav;
        _js = js;
        _nav.LocationChanged += OnLocationChanged;
    }

    private void OnLocationChanged(object? sender, LocationChangedEventArgs e)
    {
        _js.InvokeVoidAsync("clarity", "set", "page", e.Location);
    }

    public void Dispose() => _nav.LocationChanged -= OnLocationChanged;
}
