using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Razor;
using System.Linq;

namespace NorthwindIntl.ValueProviders
{
    public class ThemeViewLocationExpander : IViewLocationExpander
    {
        public string Theme {get;}

        public ThemeViewLocationExpander(string theme)
        {
            Theme = theme;
        }

        public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
        {
            var theme=context.Values["theme"];
            return viewLocations.Select(location =>location.Replace("/Views",$"/Views/{theme}"))
            .Concat(viewLocations);
        }

        public void PopulateValues(ViewLocationExpanderContext context)
        {
            context.Values["theme"]=Theme;
        }
    }
}