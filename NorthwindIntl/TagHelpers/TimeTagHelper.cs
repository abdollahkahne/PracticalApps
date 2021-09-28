using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace NorthwindIntl.TagHelpers
{
    public class CurrentTagHelper : TagHelper
    {
        // Dependency Injection By ViewContextAttribute can be used for tag helper, partials and View Components
        [ViewContext]
        private ViewContext ViewContext {get;set;}
        public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var time=DateTime.Now.ToString();
            output.Content.Append(time);
            return base.ProcessAsync(context,output);
        }
    }
}