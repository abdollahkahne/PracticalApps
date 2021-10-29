using System.Globalization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Localization.Routing;

namespace NorthwindIntl.ExceptionFilter
{
    // Middleware filters run at the same stage of the filter pipeline as Resource filters,
    //  before model binding and after the rest of the pipeline.
    public class Localization
    {
            public void Configure(IApplicationBuilder applicationBuilder)
    {
        var supportedCultures = new[]
        {
            new CultureInfo("en-US"),
            new CultureInfo("fr")
        };

        var options = new RequestLocalizationOptions
        {
            DefaultRequestCulture = new RequestCulture(
                                       culture: "en-US", 
                                       uiCulture: "en-US"),
            SupportedCultures = supportedCultures,
            SupportedUICultures = supportedCultures
        };
        options.RequestCultureProviders = new[] 
            { new RouteDataRequestCultureProvider() {
                Options = options } };

        applicationBuilder.UseRequestLocalization(options);
    }
    }
}