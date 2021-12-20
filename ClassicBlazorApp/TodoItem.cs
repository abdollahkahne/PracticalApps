namespace ClassicBlazorApp
{
    public class TodoItem
    {
        public string Title { get; set; }
        public bool IsDone { get; set; }
    }
}

// if we do not define a class inside a namespace(without namespace) it belong to global namespace and can be reachable using global::TodoItem to instantiate or inherit
// https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/namespace-alias-qualifier