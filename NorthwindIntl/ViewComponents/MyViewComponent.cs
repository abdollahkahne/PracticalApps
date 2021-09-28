using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NorthwindIntl.ValueProviders;

namespace NorthwindIntl.ViewComponents
{
    public class MyViewComponent:ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync() {
            // return Content("I am a view component");
            return new HelloWorldViewComponentResult();
        }
        
    }
}