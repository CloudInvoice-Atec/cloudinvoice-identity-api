using System.Security.Claims;
using CloudInvoice.Identity.Application.Dtos.Requests;
using CloudInvoice.Identity.Application.Dtos.Responses;
using CloudInvoice.Identity.Application.Interfaces;
using CloudInvoice.Identity.Domain.Entities;
using CloudInvoice.Identity.Domain.Interfaces;
using CloudInvoice.Identity.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;

namespace CloudInvoice.Identity.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailService _emailService;

    public AuthService(IUserRepository userRepository, ITokenService tokenService, UserManager<ApplicationUser> userManager, IEmailService emailService)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
        _userManager = userManager;
        _emailService = emailService;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto model, string scheme, string host)
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
            LastName = model.LastName,
            IsActive = true
        };

        // O Repositório trata de criar, gerar o token, montar o email e enviá-lo
        var created = await _userRepository.CreateUserAsync(user, model.Role, scheme, host);
        if (!created)
        {
            return new AuthResponseDto
            {
                IsSuccess = false,
                Message = "Erro no registo do utilizador."
            };
        }

        var token = await _tokenService.GenerateTokenAsync(user);

        return new AuthResponseDto
        {
            IsSuccess = true,
            Message = "Utilizador registado com sucesso e email de ativação enviado.",
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

        if (!user.IsActive)
        {
            return new AuthResponseDto
            {
                IsSuccess = false,
                Message = "Esta conta encontra-se inativa."
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

        var roles = await _userRepository.GetRolesAsync(user);
        var userRole = roles.FirstOrDefault() ?? string.Empty;

        var token = await _tokenService.GenerateTokenAsync(user);

        return new AuthResponseDto
        {
            IsSuccess = true,
            Message = "Login efetuado com sucesso.",
            Token = token,
            Email = user.Email,
            FullName = $"{user.FirstName} {user.LastName}",
            IsActive = user.IsActive,
            Role = userRole
        };
    }

    public async Task<AuthResponseDto> ExternalLoginAsync(string provider, ClaimsPrincipal principal)
    {
        if (string.IsNullOrWhiteSpace(provider))
        {
            return new AuthResponseDto
            {
                IsSuccess = false,
                Message = "Provider OAuth inválido."
            };
        }

        var providerKey = principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? principal.FindFirstValue("sub");
        if (string.IsNullOrWhiteSpace(providerKey))
        {
            return new AuthResponseDto
            {
                IsSuccess = false,
                Message = "Não foi possível obter a identificação externa do utilizador."
            };
        }

        var email = principal.FindFirstValue(ClaimTypes.Email)
            ?? principal.FindFirstValue("email")
            ?? principal.FindFirstValue("preferred_username");

        if (string.IsNullOrWhiteSpace(email))
        {
            return new AuthResponseDto
            {
                IsSuccess = false,
                Message = "Não foi possível obter o email do utilizador externo."
            };
        }

        var firstName = principal.FindFirstValue(ClaimTypes.GivenName) ?? principal.FindFirstValue("given_name") ?? string.Empty;
        var lastName = principal.FindFirstValue(ClaimTypes.Surname) ?? principal.FindFirstValue("family_name") ?? string.Empty;

        var user = await _userManager.FindByLoginAsync(provider, providerKey);
        user ??= await _userManager.FindByEmailAsync(email);

        if (user == null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                IsActive = true
            };

            var createResult = await _userManager.CreateAsync(user);
            if (!createResult.Succeeded)
            {
                var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
                return new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = errors
                };
            }
        }

        if (!user.IsActive)
        {
            return new AuthResponseDto
            {
                IsSuccess = false,
                Message = "Esta conta encontra-se inativa."
            };
        }

        var logins = await _userManager.GetLoginsAsync(user);
        if (!logins.Any(login => login.LoginProvider == provider && login.ProviderKey == providerKey))
        {
            var addLoginResult = await _userManager.AddLoginAsync(user, new UserLoginInfo(provider, providerKey, provider));
            if (!addLoginResult.Succeeded)
            {
                var errors = string.Join("; ", addLoginResult.Errors.Select(e => e.Description));
                return new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = errors
                };
            }
        }

        var roles = await _userRepository.GetRolesAsync(user);
        if (!roles.Any())
        {
            const string defaultRole = "Contabilista";
            if (!await _userRepository.RoleExistsAsync(defaultRole))
            {
                return new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = "A role padrão para utilizadores externos não existe no sistema."
                };
            }

            var addRoleResult = await _userManager.AddToRoleAsync(user, defaultRole);
            if (!addRoleResult.Succeeded)
            {
                var errors = string.Join("; ", addRoleResult.Errors.Select(e => e.Description));
                return new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = errors
                };
            }

            roles = await _userRepository.GetRolesAsync(user);
        }

        var token = await _tokenService.GenerateTokenAsync(user);
        var userRole = roles.FirstOrDefault() ?? string.Empty;
        var fullName = $"{user.FirstName} {user.LastName}".Trim();

        return new AuthResponseDto
        {
            IsSuccess = true,
            Message = "Login externo efetuado com sucesso.",
            Token = token,
            Email = user.Email,
            FullName = fullName,
            IsActive = user.IsActive,
            Role = userRole
        };
    }

    public Task<AuthResponseDto> LogoutAsync()
    {
        return Task.FromResult(new AuthResponseDto
        {
            IsSuccess = true,
            Message = "Logout efetuado com sucesso."
        });
    }

    public async Task<AuthResponseDto> ForgotPasswordAsync(ForgotPasswordDto model, string scheme, string host)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null || !user.IsActive)
        {
            return new AuthResponseDto
            {
                IsSuccess = true,
                Message = "Se o e-mail existir, será enviado um link para redefinir a palavra-passe."
            };
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);

        // Extrai apenas o nome do domínio/IP (removendo a porta da API se vier no 'host', ex: localhost:5001 -> localhost)
        var serverHost = host.Contains(':') ? host.Substring(0, host.IndexOf(':')) : host;

        // Constrói o link apontando explicitamente para a porta 7085 do Frontend Blazor
        var resetLink = $"{scheme}://{serverHost}:7085/account/reset-password?email={Uri.EscapeDataString(user.Email!)}&token={Uri.EscapeDataString(token)}";

        var mensagemHtml = EmailTemplates.GetPasswordResetEmail(user.FirstName, resetLink);

        await _emailService.SendEmailAsync(user.Email!, "Recuperação de Palavra-passe - CloudInvoice", mensagemHtml);

        return new AuthResponseDto
        {
            IsSuccess = true,
            Message = "Se o e-mail existir, será enviado um link para redefinir a palavra-passe."
        };
    }

    public async Task<AuthResponseDto> ResetPasswordAsync(ResetPasswordDto model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            return new AuthResponseDto { IsSuccess = false, Message = "Invalid request." };
        }

        // Corrige o sinal '+' que o browser por vezes converte em espaço no URL do token
        var decodedToken = model.Token.Replace(" ", "+");

        var result = await _userManager.ResetPasswordAsync(user, decodedToken, model.NewPassword);

        if (result.Succeeded)
        {
            return new AuthResponseDto { IsSuccess = true, Message = "Password reset successfully." };
        }

        var errors = string.Join("; ", result.Errors.Select(e => e.Description));
        return new AuthResponseDto { IsSuccess = false, Message = errors };
    }
}
