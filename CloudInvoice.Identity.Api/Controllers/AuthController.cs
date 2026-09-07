using CloudInvoice.Identity.Api.Middlewares.Exceptions;
using CloudInvoice.Identity.Application.Dtos.Requests;
using CloudInvoice.Identity.Application.Dtos.Responses;
using CloudInvoice.Identity.Application.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace CloudInvoice.Identity.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IConfiguration _configuration;

    public AuthController(IAuthService authService, IConfiguration configuration)
    {
        _authService = authService;
        _configuration = configuration;
    }

    [HttpPost("register")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var scheme = Request.Scheme;
        var host = Request.Host.Value;
        var result = await _authService.RegisterAsync(model, scheme, host);

        if (!result.IsSuccess)
        {
            // Mapear erros de negócio para exceptions
            if (result.Message.Contains("role"))
                throw new NotFoundException(result.Message);
            if (result.Message.Contains("já existe"))
                throw new ConflictException(result.Message);
            if (result.Message.Contains("Erro no registo"))
                throw new ValidationException(result.Message);

            throw new AppException(result.Message, 400);
        }

        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _authService.LoginAsync(model);

        if (!result.IsSuccess)
        {
            throw new UnauthorizedException(result.Message);
        }

        return Ok(result);
    }

    [HttpGet("external-login")]
    [HttpGet("external-login/{provider}")]
    [AllowAnonymous]
    public IActionResult ExternalLogin([FromRoute] string? provider = null, [FromQuery(Name = "provider")] string? providerQuery = null, [FromQuery] string? returnUrl = null)
    {
        provider ??= providerQuery;

        var authenticationScheme = GetExternalProviderScheme(provider);
        if (authenticationScheme is null)
        {
            return BadRequest(new AuthResponseDto
            {
                IsSuccess = false,
                Message = "Provider OAuth inválido."
            });
        }

        returnUrl ??= _configuration["Frontend:AuthCallbackUrl"];

        var callbackUrl = $"{Request.Scheme}://{Request.Host}/api/auth/external-login/{authenticationScheme}/callback";
        if (!string.IsNullOrWhiteSpace(returnUrl))
        {
            callbackUrl = QueryHelpers.AddQueryString(callbackUrl, "returnUrl", returnUrl);
        }

        var properties = new AuthenticationProperties
        {
            RedirectUri = callbackUrl
        };

        return Challenge(properties, authenticationScheme);
    }

    [HttpGet("external-login/{provider}/callback")]
    [AllowAnonymous]
    public async Task<IActionResult> ExternalLoginCallback(string provider, [FromQuery] string? returnUrl = null)
    {
        var authenticateResult = await HttpContext.AuthenticateAsync(IdentityConstants.ExternalScheme);
        try
        {
            if (!authenticateResult.Succeeded || authenticateResult.Principal is null)
            {
                return Unauthorized(new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = "Falha ao validar o login externo."
                });
            }

            var authenticationScheme = GetExternalProviderScheme(provider);
            if (authenticationScheme is null)
            {
                return BadRequest(new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = "Provider OAuth inválido."
                });
            }

            var result = await _authService.ExternalLoginAsync(authenticationScheme, authenticateResult.Principal);
            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

            returnUrl ??= _configuration["Frontend:AuthCallbackUrl"];

            if (!string.IsNullOrWhiteSpace(returnUrl))
            {
                var redirectUrl = QueryHelpers.AddQueryString(returnUrl, new Dictionary<string, string?>
                {
                    ["token"] = result.Token,
                    ["email"] = result.Email,
                    ["fullName"] = result.FullName,
                    ["role"] = result.Role
                });

                return Redirect(redirectUrl);
            }

            return Ok(result);
        }
        finally
        {
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
        }
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        var result = await _authService.LogoutAsync();
        return Ok(result);
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var scheme = Request.Scheme;
        var host = Request.Host.Value;
        var result = await _authService.ForgotPasswordAsync(model, scheme, host);

        return Ok(result);
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _authService.ResetPasswordAsync(model);

        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    private static string? GetExternalProviderScheme(string? provider)
    {
        return provider?.ToLowerInvariant() switch
        {
            "google" => GoogleDefaults.AuthenticationScheme,
            "microsoft" => MicrosoftAccountDefaults.AuthenticationScheme,
            _ => null
        };
    }
}
