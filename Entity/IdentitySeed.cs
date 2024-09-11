// using AYS.Entity;
// using AYS.Entity.Concrete;
// using Microsoft.AspNetCore.Identity;
// using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
// using Microsoft.EntityFrameworkCore;

// namespace IdentityApp.Entity
// {
//     public class IdentitySeedData
//     {
//         private const string adminUser = "admin";
//         private const string adminPassword = "admin ";

//         public static async void IdentityTestUser(IApplicationBuilder app)
//         {
//             var context = app.ApplicationServices.CreateScope().ServiceProvider.GetRequiredService<IdentityContext>();

//             if (context.Database.GetAppliedMigrations().Any())
//             {
//                 context.Database.Migrate();
//             }

//             var userManager = app.ApplicationServices.CreateScope().ServiceProvider.GetRequiredService<UserManager<AppUser>>();

//             var user = await userManager.FindByNameAsync(adminUser);
//             if (user == null)
//             {
//                 user = new AppUser
//                 {
//                     UserName = adminUser,
//                     Email = "admin@enes.com",
//                     PhoneNumber = "0000000000"
//                 };
//                 await userManager.CreateAsync(user, adminPassword);
//             }
//         }
//     }
// }







using System;
using System.Linq;
using System.Threading.Tasks;
using AYS.Entity;
using AYS.Entity.Concrete;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityApp.Entity
{
    public class IdentitySeedData
    {
        private const string AdminUserName = "admin";
        private const string AdminPassword = "admin";

        private const string CompanyUserName = "company";
        private const string CompanyPassword = "company";

        public static async void IdentityTestUser(IApplicationBuilder app)
        {
            // Create a service scope to resolve services
            using (var scope = app.ApplicationServices.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<IdentityContext>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<AppRole>>();

                // Apply pending migrations
                if (context.Database.GetPendingMigrations().Any())
                {
                    await context.Database.MigrateAsync();
                }

                // Seed roles
                if (!await roleManager.Roles.AnyAsync())
                {
                    var roles = new[]
                    {
                        new AppRole { Name = "admin" },
                        new AppRole { Name = "company" }
                    };

                    foreach (var role in roles)
                    {
                        await roleManager.CreateAsync(role);
                    }
                }

                var adminUser = await userManager.FindByNameAsync(AdminUserName);
                var companyUser = await userManager.FindByNameAsync(CompanyUserName);
                Random random = new Random();
                int randomNumber = random.Next(10000000, 100000000);
                if (adminUser == null)
                {
                    adminUser = new AppUser
                    {
                        UserName = randomNumber.ToString()+1,
                        Email = "admin@enes.com",
                        PhoneNumber = "0000010000",
                    };
                    await userManager.CreateAsync(adminUser, AdminPassword);
                }

                if (companyUser == null)
                {
                    companyUser = new AppUser
                    {
                        UserName = randomNumber.ToString(),
                        Email = "company@enes.com",
                        PhoneNumber = "0000000000",
                    };
                    await userManager.CreateAsync(companyUser, CompanyPassword);
                }

                // Ensure the admin and company users are assigned to their respective roles
                var adminRole = await roleManager.FindByNameAsync("admin");
                var companyRole = await roleManager.FindByNameAsync("company");

                if (adminRole != null && !await userManager.IsInRoleAsync(adminUser, adminRole.Name))
                {
                    await userManager.AddToRoleAsync(adminUser, adminRole.Name);
                }

                if (companyRole != null && !await userManager.IsInRoleAsync(companyUser, companyRole.Name))
                {
                    await userManager.AddToRoleAsync(companyUser, companyRole.Name);
                }
            }
        }
    }
}