using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NorthwindIntl.ModelBinders;
using NorthwindIntl.Models;

namespace NorthwindIntl.Controllers
{
    public class BookController:Controller
    {
        private IList<Book> _books=new [] {
            new Book {Id=0,Title="Modern .NET Core from Zero For Beginers!",Price=65},
            new Book {Id=1,Title="How to write clean code",Price=25,Subject=Subject.Development.ToString()},
            new Book {Id=2,Title="Design Pattern in ASP.NET",Price=60,Subject=Subject.ASP.ToString()},
            new Book {Id=3,Title="ASP.NET Core in Action",Price=40},
            new Book {Id=4,Title="Modern .NET Core",Price=65},
        };
        public IActionResult Index() {
            return View(model:_books[0]);
        }

        [HttpPost("{id}")]
        public IActionResult Index(string id,[FromForm] Book book) {
            book.Id=int.Parse(id);
            var isValid=TryValidateModel(book); // We can call this if we want to validate after changes to model
            _books.Insert(book.Id,book);
            return View(model:book);
        }

        public IActionResult UploadFile() {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UploadFile(IFormFile uploaded) {
            var file=uploaded;
            var length=file.Length;
            // var name=file.FileName; // This is unsafe to use
            // var name=file.Name; // Does not have extension
            var name=Path.GetFileName(file.FileName); //This remove untrusted / in file name (Only for logging and displating)
            var path=Path.Combine(new [] {Environment.CurrentDirectory,"wwwroot","images",$"{Path.GetRandomFileName()}{Path.GetExtension(file.FileName)}"});
            using (var fs=System.IO.File.Create(path))
            {
                 await file.CopyToAsync(fs);
            }
            return View();
        }
        
    }
}