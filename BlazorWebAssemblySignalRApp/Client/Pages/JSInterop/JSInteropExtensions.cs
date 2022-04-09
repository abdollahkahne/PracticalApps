using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazorWebAssemblySignalRApp.Client.Pages.JSInterop
{
    public static class JSInteropExtensions
    {
        public static async Task<ElementReference> ClickElem(this ElementReference element, IJSRuntime jSRuntime)
        {
            await jSRuntime.InvokeVoidAsync("JSInterop.clickElem", element);
            return element;
        }
    }
}