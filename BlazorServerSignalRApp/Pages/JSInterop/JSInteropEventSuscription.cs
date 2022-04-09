using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace BlazorServerSignalRApp.Pages.JSInterop
{
    public class JSInteropEventSuscription
    {
#nullable disable
        public event EventHandler<ElementReference> ElementHasSet;
        public void OnElementHasSet(ElementReference element)
        {
            Console.WriteLine("event raised");
            ElementHasSet?.Invoke(this, element);
        }
    }
}