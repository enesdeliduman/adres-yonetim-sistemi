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
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly IdentityContext _context;

        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IEmailSender emailSender, IdentityContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(UserLoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null)
                {
                    await _signInManager.SignOutAsync();
                    if (!await _userManager.IsEmailConfirmedAsync(user))
                    {
                        ModelState.AddModelError("", "E-posta adresiniz doğrulanmamış. Lütfen e-posta adresinizi doğrulayın.");
                        return View(model);
                    }
                    var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, true);
                    if (result.Succeeded)
                    {
                        var roleIsAdmin = await _userManager.IsInRoleAsync(user, "admin");
                        var role = roleIsAdmin ? "admin" : "company";
                        await _userManager.ResetAccessFailedCountAsync(user);
                        await _userManager.SetLockoutEndDateAsync(user, null);

                        new List<Claim>
                        {
                            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                            new Claim(ClaimTypes.Email, user.Email ?? ""),
                            new Claim(ClaimTypes.Role, role ?? "")
                        };

                        return RedirectToAction("Index", roleIsAdmin ? "Admin" : "Company");
                    }
                    else if (result.IsLockedOut)
                    {
                        var lockoutDate = await _userManager.GetLockoutEndDateAsync(user);
                        var timeLeft = lockoutDate.Value - DateTime.UtcNow;
                        ModelState.AddModelError("", $"Hesabınız kitlendi, Lütfen {timeLeft.Minutes} dakika sonra deneyiniz");
                    }
                    else
                    {
                        ModelState.AddModelError("", "parolanız hatalı");

                    }
                }
                else
                {
                    ModelState.AddModelError("", "Böyle bir hesap bulunamadı");
                }
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(UserRegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            Random random = new Random();
            int randomNumber = random.Next(10000000, 100000000);
            var user = new AppUser
            {
                UserName = randomNumber.ToString(),
                CompanyName = model.CompanyName,
                Email = model.Email
            };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                var verificationKey = await _context.VerificationKeys.AddAsync(new VerificationKey
                {
                    VerificationKeyExpirationDate = DateTime.Now.AddMonths(1),
                    VerificationKeyLine = VerificationKeyCreator.CreateVerificationKey(),
                    AppUserId = user.Id
                });
                await _userManager.AddToRoleAsync(user, "company");
                await _context.SaveChangesAsync();
                user.VerificationKeyId = verificationKey.Entity.VerificationKeyId;
                var updateResult = await _userManager.UpdateAsync(user);
                if (updateResult.Succeeded)
                {
                    TempDataHelper.SetTempDataMessage(this, "Hesabınız başarıyla oluşturuldu", "success");
                    await _emailSender.SendEmailAsync(model.Email, "Hesabınız başarıyla oluşturuldu",
                    $"Merhaba {user.CompanyName}," + "<br>"
                    + $"Hesabınız başarıyla oluşturuldu! Şu anda bir aylık deneme süresi tanımlanmıştır. Deneme süreniz <strong>{DateTime.Now.AddMonths(1).ToShortDateString()}</strong> tarihine kadar geçerli olacaktır." + "<br>"
                    + "Bu süre boyunca platformumuzu tam olarak deneyimleyebilirsiniz. Herhangi bir sorunla karşılaşırsanız, destek ekibimizle iletişime geçmekten çekinmeyin." + "<br>"
                     );

                    var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    var url = Url.Action("ConfirmEmail", "Account", new { user.Id, token });

                    await _emailSender.SendEmailAsync(model.Email, "E-posta adresi doğrulama",
                    $"Merhaba {user.CompanyName}," + "<br>"
                    + $"Hesabınızı aktifleştirmek için e-posta adresinizi doğrulamanız gerekiyor. Lütfen aşağıdaki bağlantıya tıklayarak e-posta adresinizi doğrulayın:" + "<br>"
                    + $"<a href='https://localhost:5031{url}'>Tıklayınız</a>" + "<br>"
                    + "Doğrulama işlemini gerçekleştirmeden hesabınıza giriş yapamazsınız."
                     );
                    TempDataHelper.SetTempDataMessage(this, "Hesabınız başarıyla oluşturuldu. Giriş yapmak için lütfen hesabınızı doğrulayınız", "success");
                    return RedirectToAction("Login", "Account");
                }
            }
            foreach (IdentityError err in result.Errors)
            {
                ModelState.AddModelError("", err.Description);
            }
            return View();
        }

        public async Task<IActionResult> ConfirmEmail(string Id, string Token)
        {
            if (Id == null | Token == null)
            {
                TempDataHelper.SetTempDataMessage(this, "Geçersiz token bilgisi", "warning");
                return View();
            }
            var user = await _userManager.FindByIdAsync(Id);
            if (user != null)
            {
                var result = await _userManager.ConfirmEmailAsync(user, Token);
                if (result.Succeeded)
                {
                    TempDataHelper.SetTempDataMessage(this,
                    "Hesabınız onaylandı.",
                    "success");
                    return RedirectToAction("Login");
                }
            }
            TempDataHelper.SetTempDataMessage(this, "Kullanıcı bulunamadı", "error");
            return View();
        }

        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string? Email)
        {
            if (string.IsNullOrEmpty(Email))
            {
                ModelState.AddModelError("", "Lütfen mail adresinizi giriniz");
                return View();
            }
            var user = await _userManager.FindByEmailAsync(Email);
            if (user == null)
            {
                ModelState.AddModelError("", "Boyle bir kullanici bulunamadi");
                return View();
            }
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var url = Url.Action("ResetPassword", "Account", new { user.Id, token });
            await _emailSender.SendEmailAsync(Email, "Parola Sifirlama", $"Parolanizi sifirlamak icin <a href='http://localhost:5222{url}'>linke tiklayiniz</a>");
            TempDataHelper.SetTempDataMessage(this, "Şifre sıfırlama bağlantısı gönderildi. Lütfen mail hesabınızı kontrol ediniz", "success");
            return View();
        }

        [HttpGet]
        public IActionResult ResetPassword(string Id, string Token)
        {
            if (string.IsNullOrEmpty(Id) || string.IsNullOrEmpty(Token))
            {
                return NotFound();
            }
            var model = new UserResetPasswordViewModel { Token = Token };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(UserResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(model.Id);
                if (user == null)
                {
                    return NotFound();
                }
                var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);
                if (result.Succeeded)
                {
                    TempDataHelper.SetTempDataMessage(this, "Parolanız başarıyla değiştirildi.", "success");
                    return RedirectToAction("Login");
                }
                foreach (IdentityError err in result.Errors)
                {
                    ModelState.AddModelError("", err.Description);
                }
            }
            return View(model);
        }

        [Authorize]
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _userManager.FindByIdAsync(userId);
            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (result.Succeeded)
            {
                TempDataHelper.SetTempDataMessage(this, "Parolanız başarıyla değiştirildi", "success");
                return View();
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
            return View(model);
        }

        [Authorize]
        public async Task<IActionResult> EnterVerificationKey()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _userManager.Users.Include(u => u.VerificationKey).FirstOrDefaultAsync(u => u.Id == userId);
            if (!(user.VerificationKeyId == null || user.VerificationKey == null || user.VerificationKey.VerificationKeyLine == null || DateTime.Now > user.VerificationKey.VerificationKeyExpirationDate))
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> EnterVerificationKey(EnterVerificationKeyViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var VerificationKey = await _context.VerificationKeys
                                                .Where(vk => vk.AppUserId == userId)
                                                .FirstOrDefaultAsync(vk => vk.VerificationKeyLine == model.VerificationKeyLine);

            if (VerificationKey == null)
            {
                ModelState.AddModelError("", "Hatalı doğrulama kodu");
                return View(model);
            }
            if (DateTime.Now > VerificationKey.VerificationKeyExpirationDate)
            {
                ModelState.AddModelError("", "Doğrulama kodunuzun süresi geçmiş");
                return View(model);
            }
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                ModelState.AddModelError("", "Kullanıcı bulunamadı");
                return View(model);
            }

            user.VerificationKeyId = VerificationKey.VerificationKeyId;
            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Company");
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View(model);
            }
        }

        public async Task<IActionResult> LogOut()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

    }
}
