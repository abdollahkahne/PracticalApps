using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace BlazorWebAssemblySignalRApp.Client.Pages.JSInterop
{
    public class JSInteropClass : IAsyncDisposable
    {
        private readonly Lazy<Task<IJSObjectReference>> module;

        public JSInteropClass(IJSRuntime jS)
        {
            module = new Lazy<Task<IJSObjectReference>>(() => jS.InvokeAsync<IJSObjectReference>("import", "./Pages/JSInterop/JSInteropComponent.razor.js").AsTask());
        }
        public async Task showPrompt(string msg)
        {
            // the lazy mechanism create an instance when we need it first
            var jsModule = await module.Value;
            await jsModule!.InvokeVoidAsync("showPromptLog", msg);
        }
        public async ValueTask DisposeAsync()
        {
            //Dispose if lazy mechanism created the instance
            if (module.IsValueCreated)
            {
                var jsModule = await module.Value;
                await jsModule.DisposeAsync();
            }
        }
    }
}