using AutoMapper;
using CloudInvoice.Identity.Application.Dtos.Responses;
using CloudInvoice.Identity.Application.Interfaces;
using CloudInvoice.Identity.Domain.Entities;
using CloudInvoice.Identity.Domain.Interfaces;

namespace CloudInvoice.Identity.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public UserService(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<UserResponseDto>> GetAllUsersAsync()
        {
            var usersWithRoles = await _userRepository.GetAllUsersWithRolesAsync();
            var userResponses = new List<UserResponseDto>();

            foreach (var item in usersWithRoles)
            {
                var response = _mapper.Map<UserResponseDto>(item.User);
                response.Role = item.Role;
                response.IsActive = item.IsActive;
                userResponses.Add(response);
            }

            return userResponses;
        }
        public async Task<UserResponseDto?> GetByIdAsync(string id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null) return null;

            var response = _mapper.Map<UserResponseDto>(user);
            response.Role = await GetRoleAsync(user.Id);
            return response;
        }

        // --- CREATE (Criar Utilizador) ---
        public async Task<bool> CreateUserAsync(UserResponseDto dto, string password, string scheme, string host)
        {
            var user = _mapper.Map<ApplicationUser>(dto);
            user.IsActive = dto.IsActive;
            return await _userRepository.CreateUserAsync(user, dto.Role);
        }

        public async Task<bool> UpdateUserAsync(string id, UserResponseDto dto)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null) return false;

            _mapper.Map(dto, user);
            user.IsActive = dto.IsActive;

            return await _userRepository.UpdateUserAsync(user);
        }
        // --- DELETE (Eliminar Utilizador) ---
        public async Task<bool> DeleteUserAsync(string id, string currentUserId)
        {
            if (string.Equals(id, currentUserId, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                return false;
            }

            return await _userRepository.DeleteUserAsync(id);
        }

        public async Task<string> GetRoleAsync(string userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return string.Empty;

            var roles = await _userRepository.GetRolesAsync(user);
            return roles.FirstOrDefault() ?? string.Empty;
        }

    }
}