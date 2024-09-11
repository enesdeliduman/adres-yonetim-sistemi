using System.ComponentModel.DataAnnotations;

namespace AYS.ViewModels
{
    public class ChangePasswordViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Gecerli sifre")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Yeni sifre")]
        public string NewPassword { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Compare(nameof(NewPassword), ErrorMessage = "Parola eslesmiyor")]
        [Display(Name = "Yeni sifre tekrar")]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }
}
