using System.Security.Claims;
using AutoMapper;
using CloudInvoice.Identity.Application.Dtos.Requests;
using CloudInvoice.Identity.Application.Dtos.Responses;
using CloudInvoice.Identity.Application.Email;
using CloudInvoice.Identity.Application.Interfaces;
using CloudInvoice.Identity.Domain.Entities;
using CloudInvoice.Identity.Domain.Interfaces;


namespace CloudInvoice.Identity.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly IEmailService _emailService;
    private readonly IMapper _mapper;

    public AuthService(IUserRepository userRepository, ITokenService tokenService, IEmailService emailService, IMapper mapper)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
        _emailService = emailService;
        _mapper = mapper;
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

        var user = _mapper.Map<ApplicationUser>(model);
        user.IsActive = true;

        // O Repositório trata de criar, gerar o token, montar o email e enviá-lo
        var created = await _userRepository.CreateUserAsync(user, model.Role);
        if (!created)
        {
            return new AuthResponseDto
            {
                IsSuccess = false,
                Message = "Erro no registo do utilizador."
            };
        }

        var token = await _tokenService.GenerateTokenAsync(user);
        var activationToken = await _userRepository.GeneratePasswordResetTokenAsync(user);
        var serverHost = host.Contains(':') ? host.Substring(0, host.IndexOf(':')) : host;
        var resetLink = $"{scheme}://{serverHost}:7085/auth/set-password?email={Uri.EscapeDataString(user.Email!)}&token={Uri.EscapeDataString(activationToken)}";
        var mensagemHtml = EmailTemplates.GetWelcomeEmail(user.FirstName, resetLink);
        await _emailService.SendEmailAsync(user.Email!, "Bem-vindo ao CloudInvoice!", mensagemHtml);

        var response = _mapper.Map<AuthResponseDto>(user);
        response.IsSuccess = true;
        response.Message = "Utilizador registado com sucesso e email de ativação enviado.";
        response.Token = token;

        return response;
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
        var response = _mapper.Map<AuthResponseDto>(user);
        response.IsSuccess = true;
        response.Message = "Login efetuado com sucesso.";
        response.Token = token;
        response.Role = userRole;

        return response;
    }

    public async Task<AuthResponseDto> ExternalLoginAsync(string provider, ClaimsPrincipal principal, LoginRequestDto model)
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

        const string defaultRole = "Contabilista";
        if (!await _userRepository.RoleExistsAsync(defaultRole))
        {
            return new AuthResponseDto
            {
                IsSuccess = false,
                Message = "A role padrão para utilizadores externos não existe no sistema."
            };
        }

        var user = await _userRepository.FindByLoginAsync(provider, providerKey);
        user ??= await _userRepository.GetByEmailAsync(email);

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

        if (!await _userRepository.HasLoginAsync(user, provider, providerKey))
        {
            if (!await _userRepository.AddLoginAsync(user, provider, providerKey))
            {
                return new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = "Não foi possível associar o login externo ao utilizador."
                };
            }
        }

        var roles = await _userRepository.GetRolesAsync(user);
        if (!roles.Any())
        {
            if (!await _userRepository.AddToRoleAsync(user, defaultRole))
            {
                return new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = "Não foi possível atribuir a role padrão ao utilizador externo."
                };
            }

            roles = await _userRepository.GetRolesAsync(user);
        }

        var token = await _tokenService.GenerateTokenAsync(user);
        var userRole = roles.FirstOrDefault() ?? string.Empty;
        var response = _mapper.Map<AuthResponseDto>(user);
        response.IsSuccess = true;
        response.Message = "Login externo efetuado com sucesso.";
        response.Token = token;
        response.Role = userRole;

        return response;
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
        var user = await _userRepository.GetByEmailAsync(model.Email);
        if (user == null || !user.IsActive)
        {
            return new AuthResponseDto
            {
                IsSuccess = true,
                Message = "Se o e-mail existir, será enviado um link para redefinir a palavra-passe."
            };
        }

        var token = await _userRepository.GeneratePasswordResetTokenAsync(user);

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
        var user = await _userRepository.GetByEmailAsync(model.Email);
        if (user == null)
        {
            return new AuthResponseDto { IsSuccess = false, Message = "Invalid request." };
        }

        // Corrige o sinal '+' que o browser por vezes converte em espaço no URL do token
        var decodedToken = model.Token.Replace(" ", "+");

        var result = await _userRepository.ResetPasswordAsync(user, decodedToken, model.NewPassword);

        if (result)
        {
            return new AuthResponseDto { IsSuccess = true, Message = "Password reset successfully." };
        }

        return new AuthResponseDto { IsSuccess = false, Message = "Não foi possível redefinir a palavra-passe." };
    }
}
