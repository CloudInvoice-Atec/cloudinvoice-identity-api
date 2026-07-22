using CloudInvoice.Identity.Application.Dtos;
using CloudInvoice.Identity.Application.Dtos.Responses;
using Identity.Application.DTOs.Requests;

namespace CloudInvoice.Identity.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto model);
    Task<AuthResponseDto> LoginAsync(LoginRequestDto model);
}