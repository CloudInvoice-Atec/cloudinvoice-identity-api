using CloudInvoice.Identity.Application.Dtos;
using CloudInvoice.Identity.Application.Dtos.Responses;
using CloudInvoice.Identity.Application.Interfaces;
using CloudInvoice.Identity.Domain.Entities;
using CloudInvoice.Identity.Domain.Interfaces;
using Identity.Application.DTOs.Requests;
using Microsoft.AspNetCore.Identity;

namespace CloudInvoice.Identity.Application.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ITokenService _tokenService;
    private readonly RoleManager<IdentityRole> _roleManager;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ITokenService tokenService,
        RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
        _roleManager = roleManager;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto model)
    {
        var roleExists = await _roleManager.RoleExistsAsync(model.Role);
        if (!roleExists)
        {
            return new AuthResponseDto
            {
                IsSuccess = false,
                Message = $"A role '{model.Role}' não existe no sistema."
            };
        }

        var existingUser = await _userManager.FindByEmailAsync(model.Email);
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

        var result = await _userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return new AuthResponseDto
            {
                IsSuccess = false,
                Message = $"Erro no registo: {errors}"
            };
        }

        var roleToAssign = string.Equals(model.Role, "Admin", StringComparison.OrdinalIgnoreCase) ? "Admin" : "Contabilista";

        if (await _roleManager.RoleExistsAsync(roleToAssign))
        {
            await _userManager.AddToRoleAsync(user, roleToAssign);
        }
        else
        {
            await _userManager.AddToRoleAsync(user, "Contabilista"); // Fallback de segurança
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
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            return new AuthResponseDto
            {
                IsSuccess = false,
                Message = "Credenciais inválidas ou conta inativa."
            };
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, lockoutOnFailure: false);
        if (!result.Succeeded)
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