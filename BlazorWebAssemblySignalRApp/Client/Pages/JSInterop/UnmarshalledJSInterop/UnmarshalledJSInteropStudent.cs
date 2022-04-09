using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace BlazorWebAssemblySignalRApp.Client.Pages.JSInterop.UnmarshalledJSInterop
{
#nullable disable
    [StructLayout(LayoutKind.Explicit)]
    public struct UnmarshalledJSInteropStudent
    {
        // string takes 8 bytes to save??? Not sure since string are . so we need at least offset 8 for next element (same is for long, short/ushort takes 2,int/uint takes 4 bytes)
        // This do not work as expected in class. Use struct as possible
        [FieldOffset(0)]
        public string Name;
        [FieldOffset(8)]
        public int Age;

    }
}