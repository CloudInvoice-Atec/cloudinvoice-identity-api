using CloudInvoice.Identity.Application.Interfaces;
using CloudInvoice.Identity.Domain.Interfaces;
using CloudInvoice.Identity.Infrastructure.Authentication;
using CloudInvoice.Identity.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace CloudInvoice.Identity.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ITokenService, TokenService>();

        return services;
    }
}