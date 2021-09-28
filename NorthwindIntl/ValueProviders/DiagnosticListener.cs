using System;
using Microsoft.Extensions.DiagnosticAdapter;

namespace NorthwindIntl.ValueProviders
{
    public class MyDiagnosticListener
    {
        [DiagnosticName("MyDiagnostic")]
        public virtual void OnDiagnosting(string Message) {
            Console.WriteLine(Message);
        }
    }
}