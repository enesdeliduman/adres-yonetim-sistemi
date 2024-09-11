using System.ComponentModel.DataAnnotations;

namespace AYS.ViewModels
{
    public class UserResetPasswordViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Parola eslesmiyor")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
