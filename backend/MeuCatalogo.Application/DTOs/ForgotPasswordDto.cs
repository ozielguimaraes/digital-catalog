using System.ComponentModel.DataAnnotations;

namespace MeuCatalogo.Application.DTOs;

public class ForgotPasswordDto
{
    [Required(ErrorMessage = "O identificador (email ou telefone) é obrigatório")]
    public string Identifier { get; set; }
}
