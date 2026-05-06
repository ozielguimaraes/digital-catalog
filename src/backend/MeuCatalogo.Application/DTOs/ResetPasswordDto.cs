using System.ComponentModel.DataAnnotations;

namespace MeuCatalogo.Application.DTOs;

public class ResetPasswordDto
{
    [Required(ErrorMessage = "O email/identificador é obrigatório")]
    public string Email { get; set; }

    [Required(ErrorMessage = "O token é obrigatório")]
    public string Token { get; set; }

    [Required(ErrorMessage = "A nova senha é obrigatória")]
    [StringLength(100, ErrorMessage = "A senha deve ter pelo menos {2} caracteres.", MinimumLength = 6)]
    public string NewPassword { get; set; }

    [Compare("NewPassword", ErrorMessage = "As senhas não conferem.")]
    public string ConfirmPassword { get; set; }
}
