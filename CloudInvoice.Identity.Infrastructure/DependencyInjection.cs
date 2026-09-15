using CloudInvoice.Identity.Application.Interfaces;
using CloudInvoice.Identity.Application.Services;
using CloudInvoice.Identity.Domain.Entities;
using CloudInvoice.Identity.Domain.Interfaces;
using CloudInvoice.Identity.Infrastructure.Authentication;
using CloudInvoice.Identity.Infrastructure.Data;
using CloudInvoice.Identity.Infrastructure.Repositories;
using CloudInvoice.Identity.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CloudInvoice.Identity.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequiredLength = 6;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders()
        .AddErrorDescriber<CustomIdentityErrorService>();

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IUserService, UserService>();

        services.AddScoped<IAuthService, AuthService>();
        services.AddTransient<IEmailService, EmailService>();

        return services;
    }
}