using System.Collections.Generic;
using System.Dynamic;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace NorthwindIntl.ModelBinders
{
    public static class AnynomousExtensions
    {
        public static ExpandoObject Expando(this object anynomObj) {
            var anynomDict=HtmlHelper.AnonymousObjectToHtmlAttributes(anynomObj);
            IDictionary<string,object> expanded=new ExpandoObject();
            foreach (var item in anynomDict)
            {
                expanded.Add(item);
            }
            return expanded as ExpandoObject;
        }
        
    }
}