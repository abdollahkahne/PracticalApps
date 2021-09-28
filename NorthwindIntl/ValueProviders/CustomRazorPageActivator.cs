using System.Diagnostics;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace NorthwindIntl.ValueProviders
{
    public class CustomRazorPageActivator : IRazorPageActivator
    {
        private readonly IRazorPageActivator _activator;

        public CustomRazorPageActivator(IModelMetadataProvider metadataProvider,
        IUrlHelperFactory urlHelperFactory,IJsonHelper jsonHelper,DiagnosticSource diagnosticSource,
        HtmlEncoder htmlEncoder,IModelExpressionProvider modelExpressionProvider)
        {
            _activator=new RazorPageActivator(metadataProvider,urlHelperFactory,jsonHelper,diagnosticSource,htmlEncoder,modelExpressionProvider);
        }

        public void Activate(IRazorPage page, ViewContext context)
        {
            _activator.Activate(page,context);
        
        }
    }
}