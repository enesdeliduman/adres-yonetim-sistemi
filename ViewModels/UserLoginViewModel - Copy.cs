using System.ComponentModel.DataAnnotations;

namespace AYS.ViewModels
{
    public class UserLoginViewModel
    {
        [EmailAddress]
        [Required(ErrorMessage = "Lütfen eposta adresinizi giriniz")]
        public string? Email { get; set; }

        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Lütfen şifrenizi giriniz")]
        public string? Password { get; set; }

        [Display(Name = "Beni hatırla")]
        public bool RememberMe { get; set; }
    }
}
