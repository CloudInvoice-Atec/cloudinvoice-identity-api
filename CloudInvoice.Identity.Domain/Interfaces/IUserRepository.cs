using CloudInvoice.Identity.Domain.Entities;

namespace CloudInvoice.Identity.Domain.Interfaces
{
    public interface IUserRepository
    {
        Task<ApplicationUser?> GetByIdAsync(string id);
        Task<ApplicationUser?> GetByEmailAsync(string email);
        Task<bool> CreateUserAsync(ApplicationUser user, string password);
        Task<bool> CheckPasswordAsync(ApplicationUser user, string password);
        Task<bool> UpdateUserAsync(ApplicationUser user);
    }
}