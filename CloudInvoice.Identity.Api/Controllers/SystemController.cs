using CloudInvoice.Identity.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CloudInvoice.Identity.Api.Controllers
{
    [ApiController]
    [Route("api/system")]
    public class SystemController : ControllerBase
    {
        private readonly IHealthCheckService _healthCheckService;

        public SystemController(IHealthCheckService healthCheckService)
        {
            _healthCheckService = healthCheckService;
        }

        /// <summary>
        /// Verifica se a API está online e se a Base de Dados está acessível.
        /// </summary>
        [HttpGet("health")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CheckHealth()
        {
            try
            {
                var canConnect = await _healthCheckService.CanConnectAsync();

                if (canConnect)
                {
                    return Ok(new { status = "Healthy", message = "API e Base de Dados a funcionar em pleno!" });
                }

                return StatusCode(StatusCodes.Status500InternalServerError, new { status = "Unhealthy", message = "Base de dados inacessível." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { status = "Unhealthy", error = ex.Message });
            }
        }
    }
}
