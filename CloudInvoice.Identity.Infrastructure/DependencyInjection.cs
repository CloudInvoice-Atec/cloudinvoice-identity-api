using Microsoft.Extensions.DependencyInjection;

namespace CloudInvoice.Identity.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // Aqui vais injetar os repositórios e serviços da infraestrutura
        return services;
    }
}
