using Microsoft.EntityFrameworkCore;
using AYS.Entity.Concrete;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace AYS.Entity
{
    public class IdentityContext : IdentityDbContext<AppUser, AppRole, string>
    {
        public IdentityContext(DbContextOptions<IdentityContext> options) : base(options)
        {
        }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<VerificationKey> VerificationKeys { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<VerificationKey>()
                .HasKey(vk => vk.VerificationKeyId);

            modelBuilder.Entity<VerificationKey>()
                .HasOne(vk => vk.AppUser)
                .WithOne(au => au.VerificationKey)
                .HasForeignKey<VerificationKey>(vk => vk.AppUserId);


            modelBuilder.Entity<Customer>()
                .HasOne(c => c.AppUser)
                .WithMany(u => u.Customers)
                .HasForeignKey(c => c.AppUserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Address>()
                .HasOne(a => a.Customer)
                .WithMany(c => c.Addresses)
                .HasForeignKey(a => a.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
