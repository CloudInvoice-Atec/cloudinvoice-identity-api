using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace CloudInvoice.Identity.Infrastructure.Data;

public static class RoleSeeder
{
    public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
    {
        // Resolvemos o RoleManager através do container de injeção de dependências
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        // Define os roles que queres ter no teu sistema
        string[] roles = { "Contabilista", "Utilizador"};

        foreach (var role in roles)
        {
            // Se o role ainda não existir na base de dados, é criado
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }
}