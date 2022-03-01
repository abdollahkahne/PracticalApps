// This needs dotnet run to apply changes.
// This works in both case of autostart="false" or without it
export function beforeStart(options,extensions) {
    console.log("before start blazor!");// In Blazor wasm both of options and extensios passed
    // In blazor server SignalR Circuit Start options passes
}

export function afterStarted(blazor) {
    console.log(blazor);
    console.log("After Blazor Started");
    // Here we can making JS interop calls and registering custom elements
}
