using System.ComponentModel.DataAnnotations;

namespace AYS.Entity.Concrete
{
    public class Address
    {
        [Key]
        public int AddressId { get; set; }
        public string? AddressLine { get; set; }

        public int CustomerId { get; set; }
        public Customer Customer { get; set; } = null!;
    }
}
