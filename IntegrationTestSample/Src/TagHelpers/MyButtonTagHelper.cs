using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Src.TagHelpers
{
    [HtmlTargetElement("mybutton")]
    public class MyButtonTagHelper : TagHelper
    {
        public string Type { get; set; } = "submit";
        public string BackgroundColor { get; set; } = "primary";

        public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            output.TagMode = TagMode.StartTagAndEndTag;
            output.TagName = "button";
            output.Attributes.SetAttribute("class", $"btn btn-{BackgroundColor}");
            output.Attributes.SetAttribute("type", Type);
            output.Content.SetContent("Click To Add Product");
            return base.ProcessAsync(context, output);
        }
    }
}