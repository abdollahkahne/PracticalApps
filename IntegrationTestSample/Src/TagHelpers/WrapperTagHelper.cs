using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Src.TagHelpers
{
    [HtmlTargetElement("div", Attributes = "wrap-text")]
    public class WrapperTagHelper : TagHelper
    {
        public string WrapText { get; set; }
        public bool Before { get; set; }
        public bool After { get; set; }

        public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            output.Attributes.SetAttribute("class", "m-1 p-1");
            var text = new TagBuilder("h3");
            text.InnerHtml.Append(WrapText);
            var container = new TagBuilder("div");
            // container.AddCssClass("m-1");
            // container.AddCssClass("p-1");
            container.Attributes["class"] = "bg-info mx-auto my-1 p-1";
            container.InnerHtml.AppendHtml(text);
            if (Before)
            {
                output.PreContent.AppendHtml(container);
            }
            else if (After)
            {
                output.PostContent.AppendHtml(container);
            }
            return base.ProcessAsync(context, output);
        }

    }
}