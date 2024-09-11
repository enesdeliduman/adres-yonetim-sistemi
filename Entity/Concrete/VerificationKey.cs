using System.ComponentModel.DataAnnotations;

namespace AYS.Entity.Concrete
{
    public class VerificationKey
    {
        [Key]
        public int VerificationKeyId { get; set; }
        public string? VerificationKeyLine { get; set; }
        public DateTime? VerificationKeyExpirationDate { get; set; }
        public string AppUserId { get; set; }

        public AppUser AppUser { get; set; }
    }
}
