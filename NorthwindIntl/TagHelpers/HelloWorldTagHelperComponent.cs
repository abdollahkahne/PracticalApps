using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace NorthwindIntl.TagHelpers
{
    public class HelloWorldTagHelperComponent : TagHelperComponent
    {
        public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            if (string.Equals(context.TagName,"head",System.StringComparison.OrdinalIgnoreCase)) {
                // If we use content it replace the content so use PostContent or PreContent to add html to end or start of tag!
                output.PreContent.AppendHtml("<meta name='test' content='testvalue' />");
                output.PostContent.AppendHtml("<script>window.alert('Hello Tag Helper Component');</script>");
            }
            return Task.CompletedTask;
        }
    }
}