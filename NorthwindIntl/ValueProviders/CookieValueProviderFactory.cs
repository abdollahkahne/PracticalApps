using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace NorthwindIntl.ValueProviders
{
    public class CookieValueProviderFactory : IValueProviderFactory
    {
        public Task CreateValueProviderAsync(ValueProviderFactoryContext context)
        {
            context.ValueProviders.Add(new CookieValueProvider(context.ActionContext));
            return Task.CompletedTask;
        }
    }
    public class CookieValueProvider : IValueProvider
    {
        private readonly ActionContext _context;

        public CookieValueProvider(ActionContext context)
        {
            _context = context;
        }

        public bool ContainsPrefix(string prefix)
        {
            var contains=_context.HttpContext.Request.Cookies.ContainsKey(prefix);
            return contains;
        }

        public ValueProviderResult GetValue(string key)
        {
            var cookie=_context.HttpContext.Request.Cookies[key];
            return new ValueProviderResult(cookie);
        }
    }
}