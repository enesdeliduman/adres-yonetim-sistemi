using System.ComponentModel.DataAnnotations;

namespace AYS.ViewModels
{
    public class EnterVerificationKeyViewModel
    {
        [Required(ErrorMessage = "Lutfen dogrulama kodunu giriniz")]
        [StringLength(16, MinimumLength = 16, ErrorMessage = "Dogrulama kodu tam olarak 16 karakter olmalıdır.")]
        public string? VerificationKeyLine { get; set; }
    }
}
