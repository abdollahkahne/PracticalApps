using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using NorthwindIntl.ExceptionFilter;

namespace NorthwindIntl.Controllers
{
    public class MyResponse {
        public string Name {get;set;}
    }
    [NotFound]
    public class ExceptionController:Controller
    {
        [TypeFilter(typeof(CustomExceptionHandlerFilter))]
        public IActionResult Index() {
            throw new System.Exception("This exception thrown to test Exception Filter");
        }

        [AddHeaderWithFactory]
        public object NullValue() {
            return null;
        }

        public IActionResult Test(int id) {
            return Content("This should return Not Found Filter");
        }

        [Route("{culture}/[controller]/[action]")]
        [MiddlewareFilter(typeof(Localization))]
        public IActionResult CultureFromRouteData()
        {
            return Content(
          $"CurrentCulture:{CultureInfo.CurrentCulture.Name},"
        + $"CurrentUICulture:{CultureInfo.CurrentUICulture.Name}");
        }
        
    }
}