#pragma checksum "/home/selenium/Desktop/Learning/PracticalApps/NorthwindIdentity/Views/Books/Display.cshtml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "f71f69db1ba5a83548896e9982fae28c1d176284"
// <auto-generated/>
#pragma warning disable 1591
[assembly: global::Microsoft.AspNetCore.Razor.Hosting.RazorCompiledItemAttribute(typeof(AspNetCore.Views_Books_Display), @"mvc.1.0.view", @"/Views/Books/Display.cshtml")]
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
#line 1 "/home/selenium/Desktop/Learning/PracticalApps/NorthwindIdentity/Views/_ViewImports.cshtml"
using NorthwindIdentity;

#line default
#line hidden
#nullable disable
#nullable restore
#line 2 "/home/selenium/Desktop/Learning/PracticalApps/NorthwindIdentity/Views/_ViewImports.cshtml"
using NorthwindIdentity.Models;

#line default
#line hidden
#nullable disable
#nullable restore
#line 3 "/home/selenium/Desktop/Learning/PracticalApps/NorthwindIdentity/Views/_ViewImports.cshtml"
using Microsoft.AspNetCore.Authorization;

#line default
#line hidden
#nullable disable
#nullable restore
#line 4 "/home/selenium/Desktop/Learning/PracticalApps/NorthwindIdentity/Views/_ViewImports.cshtml"
using NorthwindIdentity.AuthorizationHandler;

#line default
#line hidden
#nullable disable
#nullable restore
#line 1 "/home/selenium/Desktop/Learning/PracticalApps/NorthwindIdentity/Views/Books/Display.cshtml"
using NorthwindIdentity.Data;

#line default
#line hidden
#nullable disable
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"f71f69db1ba5a83548896e9982fae28c1d176284", @"/Views/Books/Display.cshtml")]
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"dbad708c19afa93bc2ce2d27946bfb844fc306af", @"/Views/_ViewImports.cshtml")]
    public class Views_Books_Display : global::Microsoft.AspNetCore.Mvc.Razor.RazorPage<Book>
    {
        #pragma warning disable 1998
        public async override global::System.Threading.Tasks.Task ExecuteAsync()
        {
            WriteLiteral("\n<div>\n    <h1>");
#nullable restore
#line 5 "/home/selenium/Desktop/Learning/PracticalApps/NorthwindIdentity/Views/Books/Display.cshtml"
   Write(Model.Title);

#line default
#line hidden
#nullable disable
            WriteLiteral("</h1>\n    <h2>");
#nullable restore
#line 6 "/home/selenium/Desktop/Learning/PracticalApps/NorthwindIdentity/Views/Books/Display.cshtml"
   Write(Model.Author);

#line default
#line hidden
#nullable disable
            WriteLiteral("</h2>\n</div>");
        }
        #pragma warning restore 1998
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public IAuthorizationService _auth { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.ViewFeatures.IModelExpressionProvider ModelExpressionProvider { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.IUrlHelper Url { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.IViewComponentHelper Component { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.Rendering.IJsonHelper Json { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper<Book> Html { get; private set; }
    }
}
#pragma warning restore 1591
