using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Src.TagHelpers
{
    [HtmlTargetElement("div", Attributes = "theme")]
    public class ThemeTagHelper : TagHelper
    {
        public string Theme { get; set; }
        public override void Init(TagHelperContext context)
        {
            context.Items["theme"] = Theme;
            base.Init(context);
        }
    }
    [HtmlTargetElement("a")]
    [HtmlTargetElement("button")]
    [HtmlTargetElement("mybutton")]
    public class ThemedTagHelpers : TagHelper
    {
        public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            if (context.Items.ContainsKey("theme"))
            {
                output.Attributes.SetAttribute("class", $"btn btn-{context.Items["theme"]}");
            }
            return base.ProcessAsync(context, output);
        }
    }
}