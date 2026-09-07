using CloudInvoice.Identity.Domain.Entities;

namespace CloudInvoice.Identity.Domain.Interfaces
{
    public interface IUserRepository
    {
        Task<ApplicationUser?> GetByIdAsync(string id);
        Task<ApplicationUser?> GetByEmailAsync(string email);
        Task<ApplicationUser?> FindByLoginAsync(string provider, string providerKey);
        Task<bool> CreateUserAsync(ApplicationUser user, string role);
        Task<bool> CheckPasswordAsync(ApplicationUser user, string password);
        Task<bool> UpdateUserAsync(ApplicationUser user);
        Task<bool> DeleteUserAsync(string id);
        Task<List<(ApplicationUser User, string Role, bool IsActive)>> GetAllUsersWithRolesAsync();
        Task<bool> RoleExistsAsync(string role);
        Task<bool> AddToRoleAsync(ApplicationUser user, string role);
        Task<bool> AddLoginAsync(ApplicationUser user, string provider, string providerKey);
        Task<bool> HasLoginAsync(ApplicationUser user, string provider, string providerKey);
        Task<string> GeneratePasswordResetTokenAsync(ApplicationUser user);
        Task<bool> ResetPasswordAsync(ApplicationUser user, string token, string newPassword);
        Task<IReadOnlyCollection<string>> GetRolesAsync(ApplicationUser user);
        Task<string> GetRoleForUserAsync(ApplicationUser user);
    }

}
