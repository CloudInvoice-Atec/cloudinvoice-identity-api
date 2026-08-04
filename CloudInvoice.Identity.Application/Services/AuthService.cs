using CloudInvoice.Identity.Application.Dtos.Requests;
using CloudInvoice.Identity.Application.Dtos.Responses;
using CloudInvoice.Identity.Application.Interfaces;
using CloudInvoice.Identity.Domain.Entities;
using CloudInvoice.Identity.Domain.Interfaces;

namespace CloudInvoice.Identity.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;

    public AuthService(IUserRepository userRepository, ITokenService tokenService)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto model)
    {
        var roleExists = await _userRepository.RoleExistsAsync(model.Role);
        if (!roleExists)
        {
            return new AuthResponseDto
            {
                IsSuccess = false,
                Message = $"A role '{model.Role}' não existe no sistema."
            };
        }

        var existingUser = await _userRepository.GetByEmailAsync(model.Email);
        if (existingUser != null)
        {
            return new AuthResponseDto
            {
                IsSuccess = false,
                Message = "Já existe um utilizador registado com este e-mail."
            };
        }

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            FirstName = model.FirstName,
            LastName = model.LastName
        };

        var created = await _userRepository.CreateUserAsync(user, model.Password);
        if (!created)
        {
            return new AuthResponseDto
            {
                IsSuccess = false,
                Message = "Erro no registo do utilizador."
            };
        }

        var roleToAssign = string.Equals(model.Role, "Admin", StringComparison.OrdinalIgnoreCase) ? "Admin" : "Contabilista";
        if (!await _userRepository.RoleExistsAsync(roleToAssign))
        {
            roleToAssign = "Contabilista";
        }

        var addedToRole = await _userRepository.AddToRoleAsync(user, roleToAssign);
        if (!addedToRole)
        {
            return new AuthResponseDto
            {
                IsSuccess = false,
                Message = "Utilizador criado, mas não foi possível atribuir a role."
            };
        }

        var token = await _tokenService.GenerateTokenAsync(user);

        return new AuthResponseDto
        {
            IsSuccess = true,
            Message = "Utilizador registado com sucesso.",
            Token = token,
            Email = user.Email,
            FullName = $"{user.FirstName} {user.LastName}"
        };
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto model)
    {
        var user = await _userRepository.GetByEmailAsync(model.Email);
        if (user == null)
        {
            return new AuthResponseDto
            {
                IsSuccess = false,
                Message = "Credenciais inválidas ou conta inativa."
            };
        }

        var isPasswordValid = await _userRepository.CheckPasswordAsync(user, model.Password);
        if (!isPasswordValid)
        {
            return new AuthResponseDto
            {
                IsSuccess = false,
                Message = "E-mail ou palavra-passe incorretos."
            };
        }

        var token = await _tokenService.GenerateTokenAsync(user);

        return new AuthResponseDto
        {
            IsSuccess = true,
            Message = "Login efetuado com sucesso.",
            Token = token,
            Email = user.Email,
            FullName = $"{user.FirstName} {user.LastName}"
        };
    }
}
