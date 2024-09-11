using System.Security.Claims;
using AYS.Entity;
using AYS.Entity.Concrete;
using AYS.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AYS.Controllers
{
    [Authorize(Roles = "company")]
    [ServiceFilter(typeof(VerifyKeyFilter))]
    public class CompanyController : Controller
    {
        private readonly IdentityContext _context;
        private readonly UserManager<AppUser> _userManager;

        public CompanyController(IdentityContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var customers = await _context.Customers.Where(c => c.AppUserId == userId).Include(c => c.Addresses).ToListAsync();
            ViewBag.Customers = customers;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CustomerCreate(CustomerCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }
            var customer = new Customer
            {
                Name = model.Name,
                TelephoneNumber = model.TelephoneNumber,
                Addresses = new List<Address>
                {
                    new Address
                    {
                        AddressLine = model.Address
                    }
                },
                AppUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? ""
            };
            await _context.Customers.AddAsync(customer);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> CustomerDetails(int? Id)
        {
            if (Id == null)
            {
                return NotFound();
            }
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var customer = await _context.Customers.Where(c => c.AppUserId == userId).FirstOrDefaultAsync(c => c.CustomerId == Id);
            if (customer == null)
            {
                return NotFound();
            }
            return View(customer);
        }

        [HttpPost]
        public async Task<JsonResult> AddAddressInCustomer(string? Address, int? Id)
        {
            var success = false;
            if (Id != null || !string.IsNullOrEmpty(Address))
            {
                var customer = await _context.Customers.FirstOrDefaultAsync(c => c.CustomerId == Id);
                if (customer != null)
                {
                    customer.Addresses.Add(new Address
                    {
                        AddressLine = Address
                    });
                    await _context.SaveChangesAsync();
                    success = true;
                }
            }
            return Json(new
            {
                success
            });
        }
    }
}
