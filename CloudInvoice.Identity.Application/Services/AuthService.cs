using CloudInvoice.Identity.Application.Dtos.Requests;
using CloudInvoice.Identity.Application.Dtos.Responses;
using CloudInvoice.Identity.Application.Interfaces;
using Identity.Application.DTOs.Requests;
using Microsoft.AspNetCore.Identity;

namespace CloudInvoice.Identity.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ITokenService _tokenService; // Injetamos o contrato do Token

        public AuthService(UserManager<IdentityUser> userManager, ITokenService tokenService)
        {
            _userManager = userManager;
            _tokenService = tokenService;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
        {
            var user = new IdentityUser { UserName = request.Email, Email = request.Email };
            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded) throw new Exception("Falha no registo");

            return new AuthResponseDto { Email = user.Email, Token = _tokenService.CreateToken(user) };
        }

        public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
        {
            // Validação de login com o UserManager
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
                throw new Exception("Credenciais inválidas");

            return new AuthResponseDto { Email = user.Email, Token = _tokenService.CreateToken(user) };
        }
    }
}