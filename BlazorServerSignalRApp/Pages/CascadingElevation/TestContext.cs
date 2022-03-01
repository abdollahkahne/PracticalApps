using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace BlazorServerSignalRApp.Pages.CascadingElevation
{
    public class TestContext
    {
        public string? MyProperty { get; set; }
        public Action? UpdateState { get; set; }
        public void SetMyProperty(string value)
        {
            MyProperty = value;
            UpdateState?.Invoke();
        }
    }
}