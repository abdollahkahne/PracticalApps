// <auto-generated/>
#pragma warning disable 1591
#pragma warning disable 0414
#pragma warning disable 0649
#pragma warning disable 0169

namespace ClassicBlazorApp.Pages
{
    #line hidden
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Components;
#nullable restore
#line 1 "/home/selenium/Desktop/Learning/PracticalApps/ClassicBlazorApp/_Imports.razor"
using System.Net.Http;

#line default
#line hidden
#nullable disable
#nullable restore
#line 2 "/home/selenium/Desktop/Learning/PracticalApps/ClassicBlazorApp/_Imports.razor"
using Microsoft.AspNetCore.Authorization;

#line default
#line hidden
#nullable disable
#nullable restore
#line 3 "/home/selenium/Desktop/Learning/PracticalApps/ClassicBlazorApp/_Imports.razor"
using Microsoft.AspNetCore.Components.Authorization;

#line default
#line hidden
#nullable disable
#nullable restore
#line 4 "/home/selenium/Desktop/Learning/PracticalApps/ClassicBlazorApp/_Imports.razor"
using Microsoft.AspNetCore.Components.Forms;

#line default
#line hidden
#nullable disable
#nullable restore
#line 5 "/home/selenium/Desktop/Learning/PracticalApps/ClassicBlazorApp/_Imports.razor"
using Microsoft.AspNetCore.Components.Routing;

#line default
#line hidden
#nullable disable
#nullable restore
#line 6 "/home/selenium/Desktop/Learning/PracticalApps/ClassicBlazorApp/_Imports.razor"
using Microsoft.AspNetCore.Components.Web;

#line default
#line hidden
#nullable disable
#nullable restore
#line 7 "/home/selenium/Desktop/Learning/PracticalApps/ClassicBlazorApp/_Imports.razor"
using Microsoft.AspNetCore.Components.Web.Virtualization;

#line default
#line hidden
#nullable disable
#nullable restore
#line 8 "/home/selenium/Desktop/Learning/PracticalApps/ClassicBlazorApp/_Imports.razor"
using Microsoft.JSInterop;

#line default
#line hidden
#nullable disable
#nullable restore
#line 9 "/home/selenium/Desktop/Learning/PracticalApps/ClassicBlazorApp/_Imports.razor"
using ClassicBlazorApp;

#line default
#line hidden
#nullable disable
#nullable restore
#line 10 "/home/selenium/Desktop/Learning/PracticalApps/ClassicBlazorApp/_Imports.razor"
using ClassicBlazorApp.Shared;

#line default
#line hidden
#nullable disable
    public partial class SampleComponent : Microsoft.AspNetCore.Components.ComponentBase
    {
        #pragma warning disable 1998
        protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder __builder)
        {
        }
        #pragma warning restore 1998
#nullable restore
#line 7 "/home/selenium/Desktop/Learning/PracticalApps/ClassicBlazorApp/Pages/SampleComponent.razor"
       
    [Parameter]
    public int CurrentCount { get; set; }

    private void IncrementCount()
    {
        CurrentCount++;
        Console.WriteLine("This is write by dotnet in Console.");
        Console.WriteLine($"Current Count:{CurrentCount}");
    }

#line default
#line hidden
#nullable disable
    }
}
#pragma warning restore 1591
