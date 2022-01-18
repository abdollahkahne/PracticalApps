using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorWebAssemblySignalRApp.Client.Pages.ExternalEvents
{
    public class NotifierService
    {
        public event Func<string, int, Task>? Notify; // Here we defined our type for event handler instead of default EventHandler<TEventArgs> which send Object and TEventArgs. We need the return value of Task instead of void
        public async Task OnNotify(string key, int value)
        {
            if (Notify != null)
            {
                await Notify.Invoke(key, value);
            }

        } // this is used to raise the event by this class or other derived class
    }
}

// It is the structure which used for event publisher
// 1- an event property
// 2- an event raiser method