using AYS.Entity;
using AYS.Entity.Concrete;
using AYS.Helpers;
using AYS.Models;
using AYS.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace AYS.Controllers
{
    [Authorize(Roles = "admin")]

    public class AdminController : Controller
    {

        private readonly IdentityContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly IEmailSender _emailSender;


        public AdminController(IdentityContext context, UserManager<AppUser> userManager, IEmailSender emailSender)
        {
            _context = context;
            _userManager = userManager;
            _emailSender = emailSender;
        }


        public async Task<IActionResult> Index()
        {
            ViewBag.CompanyCount = await _context.Users
                                  .Where(u => _context.UserRoles
                                  .Any(ur => ur.UserId == u.Id && _context.Roles
                                  .Any(r => r.Id == ur.RoleId && r.Name == "company")))
                                  .CountAsync();

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Companies()
        {

            var companies = await _context.Users
                                            .Where(u => _context.UserRoles
                                            .Any(ur => ur.UserId == u.Id && _context.Roles
                                            .Any(r => r.Id == ur.RoleId && r.Name == "company")))
                                            .Include(u => u.VerificationKey)
                                            .ToListAsync();

            ViewBag.ActiveCompanyCount = companies.Count(u => u.VerificationKeyId != null && u.VerificationKey.VerificationKeyExpirationDate > DateTime.Now);
            ViewBag.PassiveCompanyCount = companies.Count(u => u.VerificationKeyId == null || u.VerificationKey.VerificationKeyExpirationDate <= DateTime.Now);

            return View(companies);
        }

        [HttpGet]
        public IActionResult CreateVerificationKey()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateVerificationKey(CreateVerificationKeyViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError("", "Bu emaille bağlantılı bir hesap bulunamadı");
                return View(model);
            }
            var isVerificationKey = _context.VerificationKeys.FirstOrDefault(vk => vk.AppUserId == user.Id);
            TimeSpan RemainingUsageDays = TimeSpan.Zero;
            if (isVerificationKey != null)
            {
                if (isVerificationKey.VerificationKeyExpirationDate > DateTime.Now.AddDays(5))
                {
                    ModelState.AddModelError("", "Bu hesabin zaten en az 5 gün daha geçerli bir doğrulama kodu mevcut");
                    return View(model);
                }
                else if (
                        isVerificationKey.VerificationKeyExpirationDate < DateTime.Now.AddDays(5) &&
                        DateTime.Now < isVerificationKey.VerificationKeyExpirationDate
                        )
                {
                    RemainingUsageDays = (isVerificationKey.VerificationKeyExpirationDate ?? DateTime.Now) - DateTime.Now;
                }
            }
            if (model.Date < DateTime.Now.AddMonths(1))
            {
                ModelState.AddModelError("", "Lütfen en az 1 aylık zaman dilimi giriniz");
                return View(model);
            }
            DateTime newExpirationDate = model.Date ?? DateTime.Now;
            var message = "Doğrulama kodu oluşturuldu ve mail olarak gönderildi";
            int days = 0;
            if (RemainingUsageDays > TimeSpan.Zero)
            {
                newExpirationDate = newExpirationDate.Add(RemainingUsageDays);
                days = (int)Math.Round(RemainingUsageDays.TotalDays);
                message += $" Eski koddan kalan {days} günlük kullanım hakkı yeni koda eklendi";
            }
            var VerificationKey = VerificationKeyCreator.CreateVerificationKey();
            await _context.VerificationKeys.AddAsync(new VerificationKey
            {
                VerificationKeyExpirationDate = newExpirationDate,
                VerificationKeyLine = VerificationKey,
                AppUserId = user.Id
            });
            await _context.SaveChangesAsync();

            await _emailSender.SendEmailAsync(user.Email, "Dogrulama kodu",
                    $"Merhaba,<br><br>" +
                    $"Yeni doğrulama kodunuz başarıyla oluşturuldu.<br>" +
                    $"Doğrulama kodunuz: <strong>{VerificationKey}</strong><br>" +
                    $"Bu kod {newExpirationDate.ToShortDateString()} tarihine kadar geçerlidir.<br><br>" +
                    $"{(RemainingUsageDays > TimeSpan.Zero ? $"Eski koddan kalan {days} günlük kullanım hakkı yeni kodunuza eklenmiştir.<br>" : "")}" +
                    $"Lütfen bu kodu kullanarak hesabınızı doğrulayın.<br><br>"
                       );
            TempDataHelper.SetTempDataMessage(this, message, "success");
            return View();
        }


        // JSONS
        [HttpPost]
        public async Task<JsonResult> ResendKeyWithEmail(string? Email)
        {
            if (Email != null)
            {
                var user = await _context.Users.Where(u => u.Email == Email).Include(vk => vk.VerificationKey).FirstOrDefaultAsync();
                if (user != null)
                {
                    await _emailSender.SendEmailAsync(user.Email, "Dogrulama kodu",
                    $"Merhaba,<br><br>" +
                    $"Doğrulama kodunuz: <strong>{user.VerificationKey.VerificationKeyLine}</strong><br>" +
                    $"Bu kod {user.VerificationKey.VerificationKeyExpirationDate} tarihine kadar geçerlidir.<br><br>" +
                    $"Lütfen bu kodu kullanarak hesabınızı doğrulayın.<br><br>"
                       );
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
        public async Task<JsonResult> DeleteCompany(string? Id)
        {
            if (Id != null)
            {
                var user = await _userManager.FindByIdAsync(Id);
                if (user != null)
                {
                    await _emailSender.SendEmailAsync(user.Email, "Hesap Kapatma Bilgilendirmesi",
                    "Merhaba,<br><br>" +
                    "Hesabınızı kapatmak zorunda kaldığımızı üzüntüyle bildirmek isteriz.<br>" +
                    "En kısa zamanda görüşmek dileğiyle"
                       );
                    await _userManager.DeleteAsync(user);
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
    }
}
