using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace BlazorWebAssemblySignalRApp.Client.Pages.EventHandling
{
    public class MyCustomEventArgs : EventArgs
    {
        public string? Src { get; set; }
        public string? Value { get; set; }
    }

    public class CatDogEventArgs : EventArgs
    {
        public string? Name { get; set; }
        public string? Weight { get; set; }
        public string? Age { get; set; }
        public DateTime? Stamp { get; set; }
    }
    public class CustomPasteEventArgs : EventArgs
    {
        public DateTime EventTimeStamp { get; set; }
        public string? PastedData { get; set; }
    }
}