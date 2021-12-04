using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace Src.Data.IdentityModels
{
    // We ignore this class in db Context construction
    public class AppRole : IdentityRole // To extend Identity Role class we can do similar task like Identity User but here we should consider related cross-tabed tables like User-Role for navigation too and also add it in app db context too
    {
        // Id,Name,(Virtual) Users all properties of this 
        public string Description { get; set; }

    }
}