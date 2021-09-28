using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace NorthwindIntl.TagHelpers
{
    // we use this as <Time Format="yyyy/mm/dd">Current Date is {0}</Time>
    [OutputElementHint("span")]
    [HtmlTargetElement("time-format",TagStructure =TagStructure.NormalOrSelfClosing)]
    public class TimeFormatTagHelper : TagHelper,ITagHelper
    {
        //Define attribute as Properties (Bind automaticly)
        [HtmlAttributeName("asp-format")]
        public string Format {get;set;}
        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var content=await output.GetChildContentAsync();
            var stringContent=content.GetContent();
            var time=DateTime.Now.ToString(Format);
            output.TagName="span";
            output.Content.Append(string.Format(CultureInfo.InvariantCulture,stringContent,time));
            await base.ProcessAsync(context,output);
        }
    }
}