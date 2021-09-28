using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;

namespace NorthwindIntl.ValueProviders
{
    public class HelloWorldViewComponentResult : IViewComponentResult
    {
        public void Execute(ViewComponentContext context)
        {
            context.Writer.Write("Hello View Component Results!");
        }

        public async Task ExecuteAsync(ViewComponentContext context)
        {
            await context.Writer.WriteAsync("Hello View Component Results!");
        }
    }
}