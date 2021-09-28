using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace NorthwindIntl.RouteConstraint
{
    public class EvenIntRouteConstraint : IRouteConstraint
    {
        public bool Match(HttpContext httpContext, IRouter route, string routeKey, RouteValueDictionary values, RouteDirection routeDirection)
        {
            if (!values.ContainsKey(routeKey) || values[routeKey]==null ) {
                return false;
            }
            
            var routeValue=values[routeKey].ToString();
            var isInt=int.TryParse(routeValue,out int valueAsInt);
            if (!isInt) {return false;}
            return valueAsInt%2==0;
        }
    }
}