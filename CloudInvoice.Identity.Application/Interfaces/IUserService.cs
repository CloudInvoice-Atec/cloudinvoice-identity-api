using CloudInvoice.Identity.Application.Dtos.Responses;

namespace CloudInvoice.Identity.Application.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<UserResponseDto>> GetAllUsersAsync();
        Task<UserResponseDto?> GetByIdAsync(string id);
        Task<bool> CreateUserAsync(UserResponseDto user, string password);
        Task<bool> UpdateUserAsync(string id, UserResponseDto dto);
        Task<bool> DeleteUserAsync(string id, string currentUserId);
    }
}
