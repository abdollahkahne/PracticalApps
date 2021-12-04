using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.TagHelpers;

// we should register this using addTagHelper as two part structure which first part is wild card name space as Src.* and the second part is assembly name which is equal to project name
namespace Src.TagHelpers
{
    // The name of the attribute should be in html style like background-color. You cannot set the names of attribute starting with asp- or data-.
    // Example – when adding the attribute background-color="danger" to an HTML element. The BackgroundColor property value will be automatically assigned the value danger
    [HtmlTargetElement("button", Attributes = "background-color")]// e attribute – [HtmlTargetElement(Attributes = "background-color")] on the class tells that this Tag Helper applies to those html element that have the attribute background-color on them
    [HtmlTargetElement("a", Attributes = "background-color")]
    public class BackgroundColorTagHelper : TagHelper
    {
        public string BackgroundColor { get; set; } // The Properties inspected from html-attribute with the same name unless we want to bind to another name using HtmlAttributeName

        public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            output.PostElement.AppendHtml("<p>This is not encoded!</p>");
            output.Attributes.SetAttribute("class", $"btn btn-{BackgroundColor}");
            return base.ProcessAsync(context, output);
        }
    }
}