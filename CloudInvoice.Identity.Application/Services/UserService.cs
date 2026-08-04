using CloudInvoice.Identity.Application.Dtos.Responses;
using CloudInvoice.Identity.Application.Interfaces;
using CloudInvoice.Identity.Domain.Entities;
using CloudInvoice.Identity.Domain.Interfaces;

namespace CloudInvoice.Identity.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<IEnumerable<UserResponseDto>> GetAllUsersAsync()
        {
            // Pede os dados já processados ao repositório
            var usersWithRoles = await _userRepository.GetAllUsersWithRolesAsync();
            var userResponses = new List<UserResponseDto>();

            foreach (var item in usersWithRoles)
            {
                userResponses.Add(new UserResponseDto
                {
                    Id = item.User.Id,
                    Email = item.User.Email ?? string.Empty,
                    FirstName = item.User.FirstName,
                    LastName = item.User.LastName,
                    Role = item.Role
                });
            }

            return userResponses;
        }
        public async Task<UserResponseDto?> GetByIdAsync(string id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null) return null;

            // Converte a entidade para o DTO esperado pela interface
            return new UserResponseDto
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName,
                LastName = user.LastName,
                // Se precisares da Role, podes obtê-la ou deixá-la vazia aqui
            };
        }

        // --- CREATE (Criar Utilizador) ---
        public async Task<bool> CreateUserAsync(UserResponseDto dto, string password)
        {
            // Aqui podes colocar regras de negócio antes de criar (ex: validar se o email já existe)
            var user = new ApplicationUser
            {
                Id = dto.Id,
                Email = dto.Email,
                FirstName = dto.FirstName,
                LastName = dto.LastName
            };
            return await _userRepository.CreateUserAsync(user, password);
        }

        public async Task<bool> UpdateUserAsync(string id, UserResponseDto dto)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null) return false;

            // Atualiza os campos necessários
            user.FirstName = dto.FirstName;
            user.LastName = dto.LastName;
            user.Email = dto.Email;
            // Podes atualizar outros campos conforme o teu DTO

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

    }
}