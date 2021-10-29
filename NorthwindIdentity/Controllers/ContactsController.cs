using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NorthwindIdentity.AuthorizationHandler;
using NorthwindIdentity.Data;
using NorthwindIdentity.Models;

namespace NorthwindIdentity.Controllers
{
    public class ContactsController:Controller
    {
        private readonly IAuthorizationService _auth;
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<IdentityUser> _userManager;

        public ContactsController(IAuthorizationService auth, ApplicationDbContext dbContext, UserManager<IdentityUser> userManager)
        {
            _auth = auth;
            _dbContext = dbContext;
            _userManager = userManager;
        }

        [Authorize]
        public async Task<IActionResult> Index() {
            var contacts=_dbContext.Contact.AsQueryable();
            var isAuthorized=User.IsInRole(Roles.Admin.ToString()) || User.IsInRole(Roles.Manager.ToString());

            // Only admins can see unapproved contacts unless user be contact owner
            if (!isAuthorized) {
                contacts=contacts.Where(c => (c.Status==ContactStatus.Approved || c.OwnerID==_userManager.GetUserId(User)));
            }
            var model=await contacts.ToListAsync();
            return View(model);
        }

        [HttpGet]
        public IActionResult Create() {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Contact contact) {
            if (!ModelState.IsValid) {
                return View();
            }
            contact.OwnerID=_userManager.GetUserId(User);
            contact.Status=ContactStatus.Submited;
            var authResult=await _auth.AuthorizeAsync(user:User,resource:contact,requirement:ContactOperationsRequirements.Create);
            if (!authResult.Succeeded) {
                return Forbid();
            }

            // User has authorization And Model is valid
            _dbContext.Contact.Add(contact);
            await _dbContext.SaveChangesAsync();
            return RedirectToAction(nameof(Index));

        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id) {
            var contact=_dbContext.Contact.FirstOrDefault(c => c.ContactId==id);
            if (contact==null) {
                return NotFound();
            }

            var authResult=await _auth.AuthorizeAsync(User,contact,ContactOperationsRequirements.Update);
            if (!authResult.Succeeded) {
                return Forbid();
            }
            return View(contact);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id,Contact contact) {
            if (!ModelState.IsValid) {
                return View();
            }
            var original=_dbContext.Contact
            .AsNoTracking().FirstOrDefault(c => c.ContactId==id);
            if (original==null) {
                return NotFound();
            }
            var authResult=await _auth.AuthorizeAsync(User,original,ContactOperationsRequirements.Update);
            if (!authResult.Succeeded) {
                return Forbid();
            }
            contact.OwnerID=original.OwnerID;
            _dbContext.Attach(contact).State=EntityState.Modified; // This make an entity Trackable!

            if (original.Status==ContactStatus.Approved) {
                var isApprover=await _auth.AuthorizeAsync(User,original,ContactOperationsRequirements.Approve);
                if (!isApprover.Succeeded) {
                    // User has update permission because he is the owner 
                    contact.Status=ContactStatus.Submited;
                }
            }
            await _dbContext.SaveChangesAsync(); // This makes attached contact saved to db since it has Modified state
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id) {
            var contact=_dbContext.Contact.FirstOrDefault(c => c.ContactId==id);
            if (contact==null) {
                return NotFound();
            }

            var authResult=await _auth.AuthorizeAsync(User,contact,ContactOperationsRequirements.Read);
            if (!authResult.Succeeded ) {
                return Forbid();
            }
            return View(contact);
        }

        [HttpPost]
        [ActionName("Details")]
        public async Task<IActionResult> Approval(int id, ContactStatus status) {

            // check Model Validity
            if (!ModelState.IsValid) {
                return View();
            }
            // Get original
            var contact=_dbContext.Contact.AsNoTracking()
            .SingleOrDefault(c => c.ContactId==id);

            // Found?
            if (contact==null) {
                return NotFound();
            }

            // Authorized?
            var contactOp=contact.Status==ContactStatus.Approved?
            ContactOperationsRequirements.Reject:ContactOperationsRequirements.Approve;
            var isAuthorized=await _auth.AuthorizeAsync(User,contact,contactOp);
            if (!isAuthorized.Succeeded) {
                return Forbid();
            }

            // Update Status
            contact.Status=status;
            _dbContext.Contact.Update(contact);
            await _dbContext.SaveChangesAsync();
            return RedirectToAction(nameof(Index));


        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id) {
            var contact=_dbContext.Contact.FirstOrDefault(c => c.ContactId==id);
            if (contact==null) {
                return NotFound();
            }

            var authResult=await _auth.AuthorizeAsync(User,contact,ContactOperationsRequirements.Delete);
            if (!authResult.Succeeded) {
                return Forbid();
            }
            return View(contact);
        }

        [HttpDelete]
        [ActionName("Delete")]
        public async Task<IActionResult> PerformDelete(int id) {
            // always read original data for authorization
            var contact=_dbContext.Contact.AsNoTracking()
            .FirstOrDefault(c => c.ContactId==id);
            // check NotFound and Model Validity Always
            if (contact==null) {
                return NotFound();
            }

            // check On Demand authorization
            var isAuthorized=await _auth.AuthorizeAsync(User,contact,ContactOperationsRequirements.Delete);
            if (!isAuthorized.Succeeded) {
                return Forbid();
            }
            _dbContext.Contact.Remove(contact);
            await _dbContext.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}