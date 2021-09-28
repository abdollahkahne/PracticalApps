using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace NorthwindIntl.ValueProviders
{
    public static class HtmlHelperExtensions
    {
        public static HtmlString CurrentUser(this IHtmlHelper html) {
            return new HtmlString(html.ViewContext.HttpContext.User.Identity.Name);
        }
        
    }
}