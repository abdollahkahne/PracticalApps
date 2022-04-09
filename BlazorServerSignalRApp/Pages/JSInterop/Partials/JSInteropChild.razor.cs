using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace BlazorServerSignalRApp.Pages.JSInterop.Partials
{
    // We should implement 3 method mentioned which triggered by Observable and handled by Observers
    public partial class JSInteropChild : ComponentBase, IObserver<ElementReference>, IDisposable
    {
        [Parameter]
        public IObservable<ElementReference>? Parent { get; set; }

        // The following hold subscription return from subscribe method in observable
        private IDisposable? subscription { get; set; }
        protected override void OnParametersSet()
        {
            subscription?.Dispose();
            subscription = Parent is not null ? Parent.Subscribe(this) : null;
            base.OnParametersSet();
        }
        public void Dispose()
        {
            subscription?.Dispose();
        }

        public void OnCompleted()
        {
            subscription = null; // since we do enumerate on Observers on Observable component disposing subscription change the list and throws error
        }

        public void OnError(Exception error)
        {

            subscription = null;
        }

        public async void OnNext(ElementReference value)
        {
            Console.WriteLine($"Observed:{value.Id}");
            await value.ClickElem(JS);
        }
    }
}