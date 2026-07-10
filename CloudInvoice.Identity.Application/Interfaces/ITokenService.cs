using Microsoft.AspNetCore.Identity;

namespace CloudInvoice.Identity.Application.Interfaces
{
    public interface ITokenService
    {
        string CreateToken(IdentityUser user);
    }
}