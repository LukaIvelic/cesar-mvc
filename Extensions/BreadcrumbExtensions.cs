using Microsoft.AspNetCore.Mvc;

namespace cesar.Extensions;

public static class BreadcrumbExtensions
{
    /// <summary>
    /// Sets breadcrumb data in ViewData for rendering in _Layout
    /// </summary>
    /// <param name="controller">The controller instance</param>
    /// <param name="breadcrumbs">List of (label, controller, action) tuples</param>
    public static void SetBreadcrumbs(
        this Controller controller,
        params (string label, string controllerName, string actionName)[] breadcrumbs)
    {
        controller.ViewData["Breadcrumbs"] = breadcrumbs.ToList();
    }

    /// <summary>
    /// Sets a simple current page label (for pages with no sub-navigation)
    /// </summary>
    public static void SetCurrentPage(this Controller controller, string pageLabel)
    {
        controller.ViewData["CurrentPage"] = pageLabel;
    }
}
