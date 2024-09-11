using System.ComponentModel.DataAnnotations;
using AYS.Entity.Concrete;

namespace AYS.ViewModels
{
    public class CreateVerificationKeyViewModel
    {
        [Required(ErrorMessage = "Lütfen {0} giriniz")]
        [Display(Name = "Email")]
        [EmailAddress]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Lütfen {0} giriniz")]
        [Display(Name = "Geçerlilik tarihi")]
        public DateTime? Date { get; set; }
    }
}
