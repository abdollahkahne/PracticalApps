#pragma checksum "/home/selenium/Desktop/Learning/PracticalApps/IntegrationTestSample/Src/Views/Home/Index.cshtml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "dd98579a495d1917aca10b7442dc42e323e68e73"
// <auto-generated/>
#pragma warning disable 1591
[assembly: global::Microsoft.AspNetCore.Razor.Hosting.RazorCompiledItemAttribute(typeof(AspNetCore.Views_Home_Index), @"mvc.1.0.view", @"/Views/Home/Index.cshtml")]
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
#nullable restore
#line 1 "/home/selenium/Desktop/Learning/PracticalApps/IntegrationTestSample/Src/Views/Home/Index.cshtml"
using Src.Data.IdentityModels;

#line default
#line hidden
#nullable disable
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"dd98579a495d1917aca10b7442dc42e323e68e73", @"/Views/Home/Index.cshtml")]
    public class Views_Home_Index : global::Microsoft.AspNetCore.Mvc.Razor.RazorPage<AppUser>
    {
        #pragma warning disable 1998
        public async override global::System.Threading.Tasks.Task ExecuteAsync()
        {
            WriteLiteral("\n<h1 class=\"bg-info text-white\">Index</h1>\n");
#nullable restore
#line 5 "/home/selenium/Desktop/Learning/PracticalApps/IntegrationTestSample/Src/Views/Home/Index.cshtml"
 if (Model == null)
{

#line default
#line hidden
#nullable disable
            WriteLiteral("    <p>Hello World</p>\n");
#nullable restore
#line 8 "/home/selenium/Desktop/Learning/PracticalApps/IntegrationTestSample/Src/Views/Home/Index.cshtml"
}
else
{
    

#line default
#line hidden
#nullable disable
#nullable restore
#line 11 "/home/selenium/Desktop/Learning/PracticalApps/IntegrationTestSample/Src/Views/Home/Index.cshtml"
Write(Html.DisplayForModel());

#line default
#line hidden
#nullable disable
#nullable restore
#line 11 "/home/selenium/Desktop/Learning/PracticalApps/IntegrationTestSample/Src/Views/Home/Index.cshtml"
                           
}

#line default
#line hidden
#nullable disable
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
        public global::Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper<AppUser> Html { get; private set; }
    }
}
#pragma warning restore 1591
