using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Src.Data.TagHelperModel;

namespace Src.TagHelpers
{
    [HtmlTargetElement("product-table")]
    public class ProductTableTagHelper : TagHelper
    {
        [ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext ViewContextData { get; set; } //The ViewContext attribute denotes that the value of this property should be assigned the value of ViewContext object, when a new instance of the Tag Helper class is created.
        // The HtmlAttributeNotBound attribute tells to not assign a value to this property if there is a view-context attribute on the input HTML element.

        public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            IEnumerable<Product> products = ViewContextData.ViewData.Model as IEnumerable<Product>;
            output.TagName = "tbody";
            output.Content.SetContent("");
            foreach (var product in products)
            {
                var row = new TagBuilder("tr");
                var name = new TagBuilder("td");
                name.InnerHtml.Append(product.Name);
                row.InnerHtml.AppendHtml(name);

                var price = new TagBuilder("td");
                price.InnerHtml.Append(product.Price.ToString());
                row.InnerHtml.AppendHtml(price);

                var quantity = new TagBuilder("td");
                quantity.InnerHtml.Append(product.Quantity.ToString());
                row.InnerHtml.AppendHtml(quantity);
                output.Content.AppendHtml(row);
            }

            return base.ProcessAsync(context, output);
        }

    }
}