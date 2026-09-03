using CloudInvoice.Identity.Application.Dtos.Responses;
using CloudInvoice.Identity.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CloudInvoice.Identity.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        // --- ENDPOINT: GET /api/users ---
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UserResponseDto dto)
        {
            var success = await _userService.UpdateUserAsync(id, dto);

            if (!success)
            {
                return NotFound(new { message = "Utilizador não encontrado ou erro ao atualizar." });
            }

            return NoContent(); // Sucesso (204)
        }

        // --- ENDPOINT: DELETE /api/users/{id} ---
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(currentUserId))
            {
                return Unauthorized();
            }

            if (string.Equals(id, currentUserId, StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new { message = "Não pode eliminar o utilizador atualmente autenticado." });
            }

            var success = await _userService.DeleteUserAsync(id, currentUserId);

            if (!success)
            {
                return NotFound(new { message = "Utilizador não encontrado ou erro ao eliminar." });
            }

            return NoContent(); // Sucesso, sem conteúdo adicional (204)
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(string id)
        {
            var user = await _userService.GetByIdAsync(id);
            if (user == null) return NotFound();

            // ESTA LINHA É OBRIGATÓRIA PARA ABRIR A ROLE DA BASE DE DADOS!
            var role = await _userService.GetRoleAsync(user.Id);

            var response = new UserResponseDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                IsActive = user.IsActive,
                Role = role // <--- ESTÁS A ATRIBUIR A ROLE AQUI?
            };

            return Ok(response);
        }
    }
}