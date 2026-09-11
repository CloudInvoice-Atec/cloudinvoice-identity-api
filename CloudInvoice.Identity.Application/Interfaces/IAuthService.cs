using System.Security.Claims;
using CloudInvoice.Identity.Application.Dtos;
using CloudInvoice.Identity.Application.Dtos.Requests;
using CloudInvoice.Identity.Application.Dtos.Responses;

namespace CloudInvoice.Identity.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto model, string scheme, string host);
    Task<AuthResponseDto> LoginAsync(LoginRequestDto model);
    Task<AuthResponseDto> LogoutAsync();
    Task<AuthResponseDto> ForgotPasswordAsync(ForgotPasswordDto model, string scheme, string host);
    Task<AuthResponseDto> ResetPasswordAsync(ResetPasswordDto model);
    Task<AuthResponseDto> ExternalLoginAsync(string provider, ClaimsPrincipal principal, LoginRequestDto model);
}
