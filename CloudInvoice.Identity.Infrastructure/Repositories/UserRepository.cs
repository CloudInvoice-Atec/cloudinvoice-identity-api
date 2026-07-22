using CloudInvoice.Identity.Domain.Entities;
using CloudInvoice.Identity.Domain.Interfaces;
using CloudInvoice.Identity.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CloudInvoice.Identity.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
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
        // Aqui você pode adicionar lógica para criar o usuário com a senha
        // Por exemplo, você pode usar um serviço de hashing de senha
        // Para simplificação, vamos apenas salvar o usuário sem a senha
        await AddAsync(user);
        return true;
    }
    

    public async Task<bool> CheckPasswordAsync(ApplicationUser user, string password)
    {
        // Aqui você pode adicionar lógica para verificar a senha
        // Por exemplo, você pode usar um serviço de hashing de senha
        // Para simplificação, vamos apenas retornar true
        return true;
    }
}