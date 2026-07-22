using Microsoft.AspNetCore.Identity;

namespace CloudInvoice.Identity.Domain.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string Name { get; set; } = string.Empty;
        public string Nif { get; set; } = string.Empty;
        public string Morada { get; set; } = string.Empty;
        public string CodigoPostal { get; set; } = string.Empty;
        public string Localidade { get; set; } = string.Empty;
        public DateTime DataRegisto { get; set; } = DateTime.Now;
    }
}