using CloudInvoice.Identity.Application.Interfaces;
using CloudInvoice.Identity.Infrastructure;
using CloudInvoice.Identity.Infrastructure.Data;
using CloudInvoice.Identity.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 1. Adicionar controladores
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "CloudInvoice.Identity.Api", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Exemplo: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// 2. Chamar o método de extensão da Infraestrutura
builder.Services.AddInfrastructure(builder.Configuration);

// 3. Configurar a Autenticação (JWT + Google + Microsoft)
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["Secret"] ?? throw new InvalidOperationException("JWT Secret não configurado.");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretKey))
    };
})
.AddGoogle(googleOptions =>
{
    googleOptions.ClientId = builder.Configuration["Authentication:Google:ClientId"]!;
    googleOptions.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]!;
    googleOptions.SignInScheme = IdentityConstants.ExternalScheme;
    googleOptions.CallbackPath = "/api/auth/external-login/google/signin";
})
.AddMicrosoftAccount(microsoftOptions =>
{
    microsoftOptions.ClientId = builder.Configuration["Authentication:Microsoft:ClientId"]!;
    microsoftOptions.ClientSecret = builder.Configuration["Authentication:Microsoft:ClientSecret"]!;
    microsoftOptions.SignInScheme = IdentityConstants.ExternalScheme;
    microsoftOptions.CallbackPath = "/api/auth/external-login/microsoft/signin";
});

builder.Services.AddScoped<IHealthCheckService, HealthCheckService>();

var app = builder.Build();

app.UseStaticFiles();
app.UseMiddleware<ExceptionMiddleware>();

// 5. Executar o Seeder de Roles ao iniciar a aplicação
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await RoleSeeder.SeedRolesAsync(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Erro ao popular a base de dados.");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Importante: Authentication tem de vir SEMPRE antes de Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();