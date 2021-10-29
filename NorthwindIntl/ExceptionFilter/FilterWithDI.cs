using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;

namespace NorthwindIntl.ExceptionFilter
{
    public class FilterWithDI:ActionFilterAttribute
    {

        private readonly PositionInfo _position;

        public FilterWithDI(IOptions<PositionInfo> position) // This should be IOption to inject correctly
        {
            _position = position.Value;
            Order=1;
        }

        public override void OnResultExecuting(ResultExecutingContext context)
        {
            context.HttpContext.Response.Headers.Add(_position.Title,_position.Name);
            base.OnResultExecuting(context);
        }
    }

    public class PositionInfo {
        public string Title {get;set;}
        public string Name {get;set;}
    }
}