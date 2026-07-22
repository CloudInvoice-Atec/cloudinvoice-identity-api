using Microsoft.AspNetCore.Identity;

namespace CloudInvoice.Identity.Domain.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string Name { get; set; } = string.Empty;
    }
}