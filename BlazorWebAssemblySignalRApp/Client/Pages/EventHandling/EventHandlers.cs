using Microsoft.AspNetCore.Components;

namespace BlazorWebAssemblySignalRApp.Client.Pages.EventHandling
{
    [EventHandler("onmycustomclick", eventArgsType: typeof(MyCustomEventArgs), enablePreventDefault: true, enableStopPropagation: true)]
    [EventHandler("onmycustomcatdog", typeof(CatDogEventArgs))]
    [EventHandler("oncustompaste", typeof(CustomPasteEventArgs), enablePreventDefault: true, enableStopPropagation: true)]
    public static class EventHandlers
    // The name of class is important here
    {

    }
}