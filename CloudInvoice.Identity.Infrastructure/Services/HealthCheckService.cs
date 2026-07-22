using CloudInvoice.Identity.Application.Interfaces;
using CloudInvoice.Identity.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudInvoice.Identity.Infrastructure.Services
{
    public class HealthCheckService : IHealthCheckService
    {
        private readonly ApplicationDbContext _context;

        public HealthCheckService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> CanConnectAsync()
        {
            // Usa o mecanismo nativo do EF Core para testar a ligação à BD
            return await _context.Database.CanConnectAsync();
        }
    }
}
