using System.ComponentModel.DataAnnotations;
using AYS.Entity.Concrete;

namespace AYS.ViewModels
{
    public class CustomerViewModel
    {
        public int CustomerId { get; set; }

        [Required(ErrorMessage = "Lütfen {0} adresinizi giriniz")]
        [Display(Name = "Telefon numarası")]
        [DataType(DataType.PhoneNumber)]
        public string? TelephoneNumber { get; set; }

        [Display(Name = "İsim")]
        [Required(ErrorMessage = "Lütfen {0} giriniz")]
        public string? Name { get; set; }
    }
}
