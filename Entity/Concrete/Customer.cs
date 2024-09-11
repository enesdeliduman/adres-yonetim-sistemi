using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace AYS.Entity.Concrete
{
    public class Customer
    {
        [Key]
        public int CustomerId { get; set; }
        public string? Name { get; set; }
        public string? TelephoneNumber { get; set; }

        public string AppUserId { get; set; } = null!;
        public AppUser AppUser { get; set; } = null!;
        public ICollection<Address> Addresses { get; set; } = new List<Address>();
    }
}
