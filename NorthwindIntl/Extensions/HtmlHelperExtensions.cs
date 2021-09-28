using System;
using System.Reflection;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace NorthwindIntl.Extensions
{
    public static class  HtmlHelperExtensions
    {
        public static IHtmlContent Button(this IHtmlHelper html,string text) {
            return ButtonBuilder(text,null);
        }

        public static IHtmlContent Button(this IHtmlHelper html,string text,object htmlAttributes) {
            return ButtonBuilder(text,htmlAttributes:htmlAttributes);
        }
        private static IHtmlContent ButtonBuilder(string text,object htmlAttributes) {
            if (string.IsNullOrEmpty(text)) {
                throw new ArgumentNullException(nameof(text));
            }

            var builder=new TagBuilder("button");
            builder.InnerHtml.SetContent(text);
            if (htmlAttributes!=null) {
                foreach (var item in htmlAttributes.GetType().GetTypeInfo().GetProperties())
                {
                    builder.MergeAttribute(item.Name,item.GetValue(htmlAttributes)?.ToString()??string.Empty);
                }
            }
            return builder;
        }
    }
}