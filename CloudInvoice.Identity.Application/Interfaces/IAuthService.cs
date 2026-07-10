using CloudInvoice.Identity.Application.Dtos.Requests;
using CloudInvoice.Identity.Application.Dtos.Responses;
using Identity.Application.DTOs.Requests;

namespace CloudInvoice.Identity.Application.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request);
        Task<AuthResponseDto> LoginAsync(LoginRequestDto request);
    }
}