using System;
using Microsoft.AspNetCore.Mvc.Filters;

namespace NorthwindIntl.ExceptionFilter
{
    public class AddHeaderAttribute:ResultFilterAttribute
    {
        private readonly string _name;
        private readonly string _value;

        public AddHeaderAttribute(string name, string value)
        {
            _name = name;
            _value = value;
        }
        public override void OnResultExecuting(ResultExecutingContext context) {
            context.HttpContext.Response.Headers.Add(_name,_value);
            base.OnResultExecuting(context);// we should call this to continue the pipeline in case of sync filters
        }
    }
}