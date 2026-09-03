using CloudInvoice.Identity.Application.Interfaces;
using CloudInvoice.Identity.Domain.Entities;
using CloudInvoice.Identity.Domain.Interfaces;
using CloudInvoice.Identity.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CloudInvoice.Identity.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IEmailService _emailService;

    public UserRepository(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IEmailService emailService)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
        _emailService = emailService;
    }

    public async Task<ApplicationUser?> GetByIdAsync(string id)
    {
        return await _context.Set<ApplicationUser>().FindAsync(id);
    }

    public async Task<ApplicationUser?> GetByEmailAsync(string email)
    {
        return await _context.Set<ApplicationUser>().FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<bool> UpdateUserAsync(ApplicationUser user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CreateUserAsync(ApplicationUser user, string role, string requestScheme, string requestHost)
    {
        // 1. Cria o utilizador com uma password interna gerada apenas para satisfazer o requisito técnico da BD
        var dummyPassword = "Temp_" + Guid.NewGuid().ToString("N") + "!1A";
        var result = await _userManager.CreateAsync(user, dummyPassword);

        if (!result.Succeeded)
        {
            // Se falhar a criação base, devolve logo falso
            return false;
        }

        // 2. Removemos imediatamente essa password interna da base de dados!
        await _userManager.RemovePasswordAsync(user);

        // 3. SEGURANÇA ADICIONAL: Garante que a role existe na BD antes de a atribuir
        if (!await _roleManager.RoleExistsAsync(role))
        {
            await _roleManager.CreateAsync(new IdentityRole(role));
        }

        // 4. Adiciona a role de forma segura
        var roleResult = await _userManager.AddToRoleAsync(user, role);
        if (!roleResult.Succeeded)
        {
            // Opcional: Se quiseres apagar o utilizador caso a role falhe para não deixar lixo na BD
            // await _userManager.DeleteAsync(user);
            return false;
        }

        // 5. Gera o token oficial de definição de password (Password Reset Token)
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);

        // 6. Constrói o link limpo para o frontend onde ele vai criar a password
        var linkParaEmail = $"https://localhost:7085/auth/set-password?email={Uri.EscapeDataString(user.Email!)}&token={Uri.EscapeDataString(token)}";

        // 7. Lê o template HTML do email
        string logoUrl = $"{requestScheme}://{requestHost}/images/logo.png";
        string templatePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Templates", "WelcomeEmail.html");

        string emailTemplate = string.Empty;
        if (File.Exists(templatePath))
        {
            emailTemplate = await File.ReadAllTextAsync(templatePath);
        }
        else
        {
            emailTemplate = "<h2>Bem-vindo ao CloudInvoice</h2><p>Clique no link para criar a sua password: <a href='{{LINK}}'>Criar Password</a></p>";
        }

        // 8. Substitui as tags no HTML
        string mensagemHtml = emailTemplate
            .Replace("{{NOME}}", user.FirstName)
            .Replace("{{LOGO_URL}}", logoUrl)
            .Replace("{{LINK}}", linkParaEmail)
            .Replace("{{ANO}}", DateTime.Now.Year.ToString());

        string assunto = "Bem-vindo ao CloudInvoice! Defina a sua palavra-passe.";

        // 9. Envia o email
        try
        {
            await _emailService.SendEmailAsync(user.Email!, assunto, mensagemHtml);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AVISO] Erro ao enviar email: {ex.Message}");
        }

        return true;
    }

    public async Task<bool> CheckPasswordAsync(ApplicationUser user, string password)
    {
        return await _userManager.CheckPasswordAsync(user, password);
    }

    public async Task<bool> DeleteUserAsync(string id)
    {
        var user = await GetByIdAsync(id);
        if (user == null) return false;
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<(ApplicationUser User, string Role, bool IsActive)>> GetAllUsersWithRolesAsync()
    {
        var users = await _context.Users.ToListAsync();
        var userRoles = new List<(ApplicationUser, string, bool)>();
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? "NoRole";
            userRoles.Add((user, role, user.IsActive));
        }
        return userRoles;
    }

    public async Task<bool> RoleExistsAsync(string role)
    {
        return await _roleManager.RoleExistsAsync(role);
    }

    public async Task<bool> AddToRoleAsync(ApplicationUser user, string role)
    {
        var result = await _userManager.AddToRoleAsync(user, role);
        return result.Succeeded;
    }

    public async Task<IReadOnlyCollection<string>> GetRolesAsync(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        return roles.ToArray();
    }

    public async Task<string> GetRoleForUserAsync(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        return roles.FirstOrDefault() ?? string.Empty;
    }
}
