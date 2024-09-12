using System.Security.Claims;
using AYS.Entity;
using AYS.Entity.Concrete;
using AYS.Helpers;
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

        [HttpPost]
        public async Task<JsonResult> DeleteAddress(int AddressId)
        {
            if (AddressId != null)
            {
                var address = await _context.Addresses.FirstOrDefaultAsync(a => a.AddressId == AddressId);
                if (address != null)
                {
                    _context.Addresses.Remove(address);
                    await _context.SaveChangesAsync();
                    return Json(new
                    {
                        success = true
                    });
                }
            }
            return Json(new
            {
                success = false
            });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCustomer(int customerId)
        {
            if (customerId == null)
            {
                TempDataHelper.SetTempDataMessage(this, "Bir hata oluştu", "error");
                RedirectToAction("Details", "User", new { id = customerId });
            }
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.CustomerId == customerId);
            if (customer == null)
            {
                TempDataHelper.SetTempDataMessage(this, "Bir hata oluştu", "error");
                RedirectToAction("Details", "User", new { id = customerId });
            }
            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();

            TempDataHelper.SetTempDataMessage(this, "Müşteri başarıyla silindi.", "success");
            return RedirectToAction("Index");
        }
    }
}
