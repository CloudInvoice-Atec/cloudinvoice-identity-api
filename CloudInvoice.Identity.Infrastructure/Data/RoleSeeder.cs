using CloudInvoice.Identity.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace CloudInvoice.Identity.Infrastructure.Data;

public static class RoleSeeder
{
    public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        // 1. Garantir que a role Admin existe
        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new IdentityRole("Admin"));
        }

        // 2. Garantir que a role Contabilista existe
        if (!await roleManager.RoleExistsAsync("Contabilista"))
        {
            await roleManager.CreateAsync(new IdentityRole("Contabilista"));
        }

        // 3. Criar o utilizador Admin se não existir
        var adminEmail = "admin@cloudinvoice.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "Admin",
                LastName = "CloudInvoice",
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(adminUser, "Atec@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
    }
}