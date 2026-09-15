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

    /// <summary>
    /// Regista um novo utilizador com base no modelo fornecido. Apenas utilizadores com a role "Admin" podem aceder a este endpoint.
    /// </summary>
    /// <param name="model"></param>
    /// <returns>Um objeto AuthResponseDto com o resultado da operação.</returns>
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

    /// <summary>
    /// Autentica um utilizador com base no modelo fornecido. Retorna um token JWT se a autenticação for bem-sucedida.
    /// </summary>
    /// <param name="model"></param>
    /// <returns>Um objeto AuthResponseDto com o resultado da operação.</returns>
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

    /// <summary>
    /// Inicia o processo de login externo com base no provedor especificado. Redireciona o utilizador para a página de autenticação do provedor externo.
    /// </summary>
    /// <returns>Um objeto IActionResult que representa a resposta da requisição.</returns>
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

    /// <summary>
    /// Callback para o login externo. Este endpoint é chamado pelo provedor externo após a autenticação do utilizador. Processa a resposta do provedor e retorna um token JWT se a autenticação for bem-sucedida.
    /// </summary>
    /// <returns>Um objeto IActionResult que representa a resposta da requisição.</returns>
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
            var model = new LoginRequestDto();

            var authenticationScheme = GetExternalProviderScheme(provider);
            if (authenticationScheme is null)
            {
                return BadRequest(new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = "Provider OAuth inválido."
                });
            }

            var result = await _authService.ExternalLoginAsync(authenticationScheme, authenticateResult.Principal, model);

            if (!result.IsSuccess)
            {
                // REDIRECIONAMENTO DIRETO PARA O LOGIN DO BLAZOR
                var loginPageUrl = "https://localhost:7085/";

                string mensagemRealDaApi = string.IsNullOrWhiteSpace(result.Message)
                    ? "Falha ao iniciar sessão com o fornecedor externo."
                    : result.Message;

                var errorRedirectUrl = QueryHelpers.AddQueryString(loginPageUrl, "apiError", mensagemRealDaApi);

                return Redirect(errorRedirectUrl);
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

    /// <summary>
    /// Encerra a sessão do utilizador autenticado. Este endpoint requer autenticação e invalida o token JWT atual.
    /// </summary>
    /// <returns>Um objeto IActionResult que representa a resposta da requisição.</returns>
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        var result = await _authService.LogoutAsync();
        return Ok(result);
    }

    /// <summary>
    /// Inicia o processo de recuperação de senha para um utilizador com base no modelo fornecido. Envia um email com instruções para redefinir a senha.
    /// </summary>
    /// <param name="model"></param>
    /// <returns>Um objeto IActionResult que representa a resposta da requisição.</returns>
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

    /// <summary>
    /// Redefine a senha de um utilizador com base no modelo fornecido. Este endpoint é chamado após o utilizador clicar no link de redefinição de senha enviado por email.
    /// </summary>
    /// <param name="model"></param>
    /// <returns>Um objeto IActionResult que representa a resposta da requisição.</returns>
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
