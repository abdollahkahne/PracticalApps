using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorServerSignalRApp.Data
{
    public class TokenProvider
    {
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public TokenProvider() { }
    }
    public class InitialApplicationState
    {
        public TokenProvider? Tokens { get; set; }
        public string? TraceIdentifier { get; set; }
        public long? IP { get; set; }
        public int LocalPort { get; set; }
    }
}