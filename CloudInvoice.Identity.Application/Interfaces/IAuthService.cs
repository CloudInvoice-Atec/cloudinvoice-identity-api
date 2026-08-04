using CloudInvoice.Identity.Application.Dtos;
using CloudInvoice.Identity.Application.Dtos.Requests;
using CloudInvoice.Identity.Application.Dtos.Responses;

namespace CloudInvoice.Identity.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto model);
    Task<AuthResponseDto> LoginAsync(LoginRequestDto model);
}