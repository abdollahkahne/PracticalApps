using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using NorthwindIntl.ActionConstraints;
using NorthwindIntl.ValueProviders;
using NorthwindIntl.ExceptionFilter;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Caching.Redis;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Net;

namespace NorthwindIntl.Controllers
{
    public class CalculatorController:Controller
    {
        public override void OnActionExecuting(ActionExecutingContext context) {
            //This will be execute before action(synchronously)
            Console.WriteLine("I executed before starting action");
        }

        // [ActionName("CalculateByMethod")]
        public int CalculateA(string id,int a,int b) {
            return id switch {
                "Add"=>a+b,
                "Multiply"=>a*b,
                "Divide"=>a/b,
                "Reminder"=>a%b,
                "Minus"=>a-b,
                "Length"=>(int)Math.Sqrt(a^2+b^2),
                _=>0
            };
        }

        // [ActionName("Add")]
        // [HttpGet("Calculate({a:int},{b:int})")]
        public object Calculate(int a,int b) {
            // throw new Exception("This is a test exception");
            var dataToken=HttpContext.GetEndpoint().Metadata.GetMetadata<IDataTokensMetadata>();
            var foo =dataToken.DataTokens["Foo"] as string;
            return new {
                Foo ="foo",
                Sum=a+b,
            };
        }

        // [CustomAuthorization]
        // [HttpPost]
        // public JsonResult FormSubmit(IFormFileCollection files) {
        //     foreach (var file in files)
        //     {
        //         Console.WriteLine(file.FileName);
        //     }
        //     return Json(new {
        //         success=true,
        //     });
        // }

        // public JsonResult Process([CustomHtmlEncode] string html) {
        //     return Json(new {
        //         encoded=html
        //     });
        // }

        // public IActionResult InlineValidate(string id) {
        //     var valid=TryValidateModel(id);
        //     if (valid) {
        //         return Ok();
        //     } else {
        //         return BadRequest();
        //     }

        // }

        public IActionResult Subscribe([Required,EmailAddress] string email) {
            if (ModelState.IsValid) {
                return Ok();
            }
            return BadRequest();

        }

        public IActionResult CheckMarriedValidation(Person person) {
            Console.WriteLine("I am action");
            if (ModelState.IsValid) {
                return new XMLResult(person);
            }
            Console.WriteLine(ModelState.GetFieldValidationState("person"));
            return Json(new {success=false});
            
        }

        public FileStreamResult StreamSong(string name) {
            var path=System.IO.Path.Combine(Environment.CurrentDirectory,"wwwroot/sounds/1.mp3");
            var stream=System.IO.File.OpenRead(path);
            return new FileStreamResult(stream,"Audio/mp3");
        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            Console.WriteLine("I executed after finishing action");
            // This will execute after action
        }

        [CustomExceptionFilter]
        public void ExceptionPage() {
            throw new Exception("This is a test exception for checking Exception Filters!");
        }

        [ResponseCache(CacheProfileName="MyCacheProfile")]
        public JsonResult Cache() {
            return Json(new {
                Message="I am cached using cache profile",
                CurrentTime=DateTime.UtcNow,
            });
        }

        [ResponseCache(Duration=30)]
        public JsonResult CacheWithoutProfile() {
            return Json(new {
                Message="I am cached without using cache profile",
                CurrentTime=DateTime.UtcNow,
            });
        }

        public JsonResult Cookie() {
            HttpContext.Response.Cookies.Append("username","alfi",new CookieOptions {
                Domain="localhost",
                Expires=DateTimeOffset.Now.AddMinutes(2),
                Secure=false,
                HttpOnly=true,
                Path="/",
            });
            return Json(new {
                Message="I am cached without using cache profile",
                CurrentTime=DateTime.UtcNow,
            });
        }

        public JsonResult Session() {
            var sessionID=HttpContext.Session.Id;
            var containsKey=HttpContext.Session.TryGetValue("key",out byte[] sessionKey);
            if (containsKey) {
                return Json(new {
                    sessionID,
                    sessionKey=sessionKey.ToString()
                });
            } else {
                HttpContext.Session.SetString("key","Sample Value");
                return Json(new {
                    sessionID,
                    Message="Session is not set yet!",
                });
            }
        }

        public IActionResult MemoryCache([FromServices] IMemoryCache cache) {
            Person person=new Person {
                FullName="Ali",
                Married=true,
                PartnerName="Fateme",
            };
            
            var cached=cache.TryGetValue("person",out Person p);
            if (cached) {
                return Json(new {
                    FromCache=true,
                    Person=p,
                });
            } else {
                cache.Set<Person>("person",person);
                return Json(new {FromCache=false,});
            }
            

        }

        public IActionResult DistributedCache([FromServices] IDistributedCache cache) {
            Person person=new Person {
                FullName="Ali",
                Married=true,
                PartnerName="Fateme",
            };

            var cachedAsString=cache.GetString("person");
            
            if (!string.IsNullOrEmpty(cachedAsString)) {
                Person p=JsonSerializer.Deserialize<Person>(cachedAsString);
                
                return Json(new {
                    FromCache=true,
                    Person=p,
                });
            } else {
                var personAsString=JsonSerializer.Serialize(person);
                cache.SetStringAsync("person",personAsString);
                return Json(new {FromCache=false,});
            }
            

        }

        public JsonResult SaveTempData() {
            var person=TempData;
            return Json(person.Count);
        }


        // [Host("localhost","127.0.0.1")]
        // public IActionResult Local() {
        //     return NoContent();
        // }

        // [Host("0:8080")]
        // public IActionResult Port80() {
        //     return NoContent();
        // }

        public IActionResult TestViewComponent() {
            return ViewComponent("My",new {p1=4,p2=true}); // return view component from action
        }

    }
}