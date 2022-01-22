using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace BlazorWebAssemblySignalRApp.Client.Pages.Tabs
{
    public interface ITab
    {
        RenderFragment? ChildContent { get; }
    }
}