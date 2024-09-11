using Microsoft.AspNetCore.Identity;

namespace AYS.Entity.Concrete
{
    public class AppUser : IdentityUser
    {
        public string? CompanyName { get; set; }
        public int? VerificationKeyId { get; set; }
        public VerificationKey VerificationKey { get; set; }
        public List<Customer> Customers { get; set; } = new List<Customer>();
    }
}