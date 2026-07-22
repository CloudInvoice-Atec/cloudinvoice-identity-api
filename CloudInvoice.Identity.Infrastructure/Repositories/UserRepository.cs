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

    public UserRepository(ApplicationDbContext context, UserManager<ApplicationUser> userManager    )
    {
        _context = context;
        _userManager = userManager;
    }
    public async Task<ApplicationUser?> GetByIdAsync(string id)
    {
        // Forçamos o EF a usar o DbSet correto do ApplicationUser
        return await _context.Set<ApplicationUser>().FindAsync(id);
    }

    public async Task<ApplicationUser?> GetByEmailAsync(string email)
    {
        // Usamos também o Set<ApplicationUser>() aqui
        return await _context.Set<ApplicationUser>().FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task AddAsync(ApplicationUser user)
    {
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> UpdateUserAsync(ApplicationUser user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CreateUserAsync(ApplicationUser user, string password)
    {
        // Usa o UserManager para criar o utilizador e fazer o hash seguro da password
        var result = await _userManager.CreateAsync(user, password);
        return result.Succeeded;
    }

    public async Task<bool> CheckPasswordAsync(ApplicationUser user, string password)
    {
        // Valida se a password inserida bate certo com o hash guardado na base de dados
        var result = await _userManager.CheckPasswordAsync(user, password);
        return result;
    }
}