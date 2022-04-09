using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace BlazorServerSignalRApp.Pages.JSInterop.HelperClass
{
    public class HelperClassJSInterop
    {
        private readonly Action _action;

        public HelperClassJSInterop(Action action)
        {
            _action = action;
        }

        [JSInvokable]
        public void changeContent()
        {
            _action.Invoke();
        }
    }
}