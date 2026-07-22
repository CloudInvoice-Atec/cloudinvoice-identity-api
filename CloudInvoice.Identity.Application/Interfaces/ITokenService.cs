using CloudInvoice.Identity.Domain.Entities;

namespace CloudInvoice.Identity.Application.Interfaces;

public interface ITokenService
{
    Task<string> GenerateTokenAsync(ApplicationUser user);
}