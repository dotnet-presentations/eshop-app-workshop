using Microsoft.AspNetCore.Components;

namespace Microsoft.AspNetCore.Components;

public static class NavigationManagerExtensions
{
    public static void NavigateTo(this NavigationManager navigationManager, string url, HttpContext httpContext, bool forceFullNavigation)
    {
        if (!forceFullNavigation)
        {
            navigationManager.NavigateTo(url);
            return;
        }

        // Workaround to force Blazor enhanced navigation to do a full redirect to an internal URL from an enhanced form handler
        if (!httpContext.Response.HasStarted)
        {
            httpContext.Response.Headers["blazor-enhanced-nav-redirect-location"] = url;
        }
        else
        {
            throw new InvalidOperationException("Cannot force a full redirect when response has already started, e.g. when Streaming Rendering is enabled.");
        }
    }
}
