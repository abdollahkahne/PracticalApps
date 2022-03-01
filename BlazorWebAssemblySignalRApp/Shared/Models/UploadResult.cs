using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorWebAssemblySignalRApp.Shared.Models
{
    public class UploadResult
    {
        public bool Uploaded { get; set; }
        public String? FileName { get; set; }
        public String? StoredFileName { get; set; }
        public int? ErrorCode { get; set; } // null indicate not specified error
    }
}