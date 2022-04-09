using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace BlazorServerSignalRApp.Pages.JSInterop.Partials
{
    // partial class always should implement component base
    public partial class JSInteropParent : ComponentBase, IDisposable, IObservable<ElementReference>
    {
        private ElementReference title { get; set; }
        // save list of observers and we should have a mechanism to dispose this list managed by framework
        private IList<IObserver<ElementReference>> observers = new List<IObserver<ElementReference>>();
        private bool disposing { get; set; }

        // we should raise onNext event for observers in OnAfterRender
        protected override void OnAfterRender(bool firstRender)
        {
            if (firstRender)
            {
                foreach (var item in observers)
                {
                    try
                    {
                        Console.WriteLine($"{title.Id} Pushed to Observer {item.ToString()}");
                        // Provides the observer with new data
                        item.OnNext(title);
                    }
                    catch (System.Exception ex)
                    {
                        Console.WriteLine($"On After Render Exception {ex.Message}");
                    }
                }
            }

            base.OnAfterRender(firstRender);
        }

        //Here we clear the list and raise OnComplete event for observers
        public void Dispose()
        {
            disposing = true;
            // To implement an Observer we should implement three method for it (next,error,complete) and observable should raise them. Here we try to complete 
            foreach (var item in observers)
            {
                try
                {
                    // Notifies the observer that the provider has finished sending push-based notifications.
                    item.OnCompleted();
                }
                catch (System.Exception ex)
                {
                    // Console.WriteLine(ex.Message);
                }
            }
            observers.Clear();
        }

        public IDisposable Subscribe(IObserver<ElementReference> observer)
        {
            if (disposing)
            {
                throw new InvalidOperationException("The Observable being disposed");
            }
            observers.Add(observer);
            return new Subscription(this, observer);
            // we should return a disposable class responsible for disposing connection between Observers and Observable
        }

        // This class dispose the created connection between observable and observer by framework
        private class Subscription : IDisposable
        {
            public Subscription(JSInteropParent observable, IObserver<ElementReference> observer)
            {
                this.Observable = observable;
                Observer = observer;
            }

            public JSInteropParent Observable { get; set; }
            public IObserver<ElementReference> Observer { get; set; }

            // Here we only remove the specified subscription from framework
            public void Dispose()
            {
                Observable.observers.Remove(Observer);
            }
        }
    }
}