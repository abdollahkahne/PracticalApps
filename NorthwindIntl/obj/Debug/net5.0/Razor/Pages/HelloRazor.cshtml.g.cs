#pragma checksum "/home/selenium/Desktop/Learning/PracticalApps/NorthwindIntl/Pages/HelloRazor.cshtml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "45378d62d2b6a6f5d55a35f8c2d46e2bce5a5730"
// <auto-generated/>
#pragma warning disable 1591
[assembly: global::Microsoft.AspNetCore.Razor.Hosting.RazorCompiledItemAttribute(typeof(AspNetCore.Pages_HelloRazor), @"mvc.1.0.razor-page", @"/Pages/HelloRazor.cshtml")]
namespace AspNetCore
{
    #line hidden
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.AspNetCore.Mvc.ViewFeatures;
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorCompiledItemMetadataAttribute("RouteTemplate", "{id?}")]
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"45378d62d2b6a6f5d55a35f8c2d46e2bce5a5730", @"/Pages/HelloRazor.cshtml")]
    public class Pages_HelloRazor : global::Microsoft.AspNetCore.Mvc.RazorPages.Page
    {
        #pragma warning disable 1998
        public async override global::System.Threading.Tasks.Task ExecuteAsync()
        {
            WriteLiteral("\n<p>\n    To use Route Values we should use <code>RouteData.Values[\"id\"]</code>.For example, this page has id equal to ");
#nullable restore
#line 4 "/home/selenium/Desktop/Learning/PracticalApps/NorthwindIntl/Pages/HelloRazor.cshtml"
                                                                                                            Write(RouteData.Values["id"]);

#line default
#line hidden
#nullable disable
            WriteLiteral("</p>\n<p>\n    To get query string we should use <code>Request.Query[\"q\"]</code>,for example: ");
#nullable restore
#line 6 "/home/selenium/Desktop/Learning/PracticalApps/NorthwindIntl/Pages/HelloRazor.cshtml"
                                                                              Write(Request.Query["q"]);

#line default
#line hidden
#nullable disable
            WriteLiteral("\n</p>");
        }
        #pragma warning restore 1998
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.ViewFeatures.IModelExpressionProvider ModelExpressionProvider { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.IUrlHelper Url { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.IViewComponentHelper Component { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.Rendering.IJsonHelper Json { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper<Pages_HelloRazor> Html { get; private set; }
        public global::Microsoft.AspNetCore.Mvc.ViewFeatures.ViewDataDictionary<Pages_HelloRazor> ViewData => (global::Microsoft.AspNetCore.Mvc.ViewFeatures.ViewDataDictionary<Pages_HelloRazor>)PageContext?.ViewData;
        public Pages_HelloRazor Model => ViewData.Model;
    }
}
#pragma warning restore 1591