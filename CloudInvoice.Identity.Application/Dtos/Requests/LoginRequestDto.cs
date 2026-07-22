using System.ComponentModel.DataAnnotations;

namespace Identity.Application.DTOs.Requests
{
    public class LoginRequestDto
    {
        [Required(ErrorMessage = "Campo obrigatório.")]
        [EmailAddress(ErrorMessage = "Formato de email inválido.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Campo obrigatório.")]
        public string Password { get; set; } = string.Empty;
    }
}