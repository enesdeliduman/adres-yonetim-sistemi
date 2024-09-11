using System.ComponentModel.DataAnnotations;

namespace AYS.ViewModels
{
    public class UserRegisterViewModel
    {
        [Required(ErrorMessage = "Lütfen {0} giriniz")]
        [Display(Name = "Şirket ismi")]
        public string? CompanyName { get; set; }

        [EmailAddress]
        [Required(ErrorMessage = "Lütfen {0} giriniz")]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Lütfen {0} giriniz")]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre")]
        public string? Password { get; set; }

        [Required(ErrorMessage = "Lütfen {0} giriniz")]
        [Compare(nameof(Password), ErrorMessage = "Parola eslesmiyor")]
        [DataType(DataType.Password)]
        [Display(Name = "Yeni sifre tekrar")]
        public string ConfirmPassword { get; set; } = string.Empty;

    }
}
