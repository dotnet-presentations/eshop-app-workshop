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
        httpContext.Response.Headers["blazor-enhanced-nav-redirect-location"] = url;
    }
}
