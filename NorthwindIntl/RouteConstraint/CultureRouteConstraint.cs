using System.Globalization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace NorthwindIntl.RouteConstraint
{
    public class CultureRouteConstraint : IRouteConstraint
    {
        public const string CultureKey="culture";
        public bool Match(HttpContext httpContext, IRouter route, string routeKey, RouteValueDictionary values, RouteDirection routeDirection)
        {
            // Check the culture value set
            if (!values.ContainsKey(CultureKey)||values[CultureKey]==null) {return false;}

            //Get culture route parameter and requestLocalizationOptions (for getting supported cultures)
            var culture=values[CultureKey].ToString();
            var supportedCultures=httpContext.RequestServices.GetRequiredService<RequestLocalizationOptions>().SupportedCultures;

            // check if provided culture parameter is valid?
            if (supportedCultures==null||supportedCultures.Count==0) {
                try
                {
                     new CultureInfo(culture);
                     return true;
                }
                catch (System.Exception)
                {
                    
                    return false;
                }
            }
            return supportedCultures.Any(c =>c.Name.Equals(culture,System.StringComparison.CurrentCultureIgnoreCase));
        }
    }
}