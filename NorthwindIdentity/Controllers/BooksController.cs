using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NorthwindIdentity.AuthorizationHandler;
using NorthwindIdentity.Data;

namespace NorthwindIdentity.Controllers
{
    public class BooksController:Controller
    {
        private List<Book> _books;
        private readonly IAuthorizationService _authSvc;

        public BooksController(IAuthorizationService authSvc)
        {
            _books = new List<Book>();
            _books.Add(new Book
            {
                Id = 1,
                Title = "Sample Book 1",
                Author = "aaaa@gmail.com"
            });
            _books.Add(new Book
            {
                Id = 2,
                Title = "Sample Book 2",
                Author = "bbbbb@yahoo.com"
            });
            _books.Add(new Book
            {
                Id = 3,
                Title = "Sample Book 3",
                Author = "cccccc@gmail.com"
            });
            _authSvc = authSvc;
        }

        // Resource-Based Authentication:
        // Authorization strategy depends upon the resource being accessed.
        // Consider a document that has an author property.
        // Only the author is allowed to update the document.
        // Consequently, the document must be retrieved from the data store
        // before authorization evaluation can occur
        public async Task<IActionResult> Display(int id) {
            var book=_books.SingleOrDefault(b =>b.Id==id);
            var authorizationResult=await _authSvc.AuthorizeAsync(
                User,
                book,
                new SameAuthorAuthorizationRequirement()
            );
            if (authorizationResult.Succeeded)
            {
                return View(model:book);
            }
            else if (User.Identity.IsAuthenticated)
            {
                return new ForbidResult();
            }
            else
            {
                return new ChallengeResult();
            }
            
        }
    }
}