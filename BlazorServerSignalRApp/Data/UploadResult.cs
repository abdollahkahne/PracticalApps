using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorServerSignalRApp.Data
{
    public class UploadResult
    {
        public bool Uploaded { get; set; }
        public String? FileName { get; set; }
        public String? StoredFileName { get; set; }
        public int ErrorCode { get; set; }
    }
}