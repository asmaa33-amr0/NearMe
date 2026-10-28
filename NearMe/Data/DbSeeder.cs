using Microsoft.AspNetCore.Identity;
using NearMe.Models;

namespace NearMe.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

            foreach (var role in Roles.All)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            var email = "admin@nearme.com";
            var password = "Admin@123";

            Console.WriteLine(">>> SEED: started");

            var existing = await userManager.FindByEmailAsync(email);
            if (existing != null)
            {
                Console.WriteLine(">>> SEED: admin already exists, id = " + existing.Id);
                return;
            }

            var admin = new ApplicationUser
            {
                UserName = email,
                Email = email,
                fullname = "Admin",
                CreatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(admin, password);
            Console.WriteLine(">>> SEED: create succeeded = " + result.Succeeded);

            if (!result.Succeeded)
            {
                foreach (var e in result.Errors)
                    Console.WriteLine(">>> SEED ERROR: " + e.Description);
                return;
            }

            var roleResult = await userManager.AddToRoleAsync(admin, Roles.Admin);
            Console.WriteLine(">>> SEED: role assigned = " + roleResult.Succeeded);
        }
    }
}