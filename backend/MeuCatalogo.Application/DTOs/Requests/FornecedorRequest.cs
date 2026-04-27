namespace MeuCatalogo.Application.DTOs.Requests;

public class FornecedorRequest
{
    public string Nome { get; set; } = string.Empty;
    public string? Categoria { get; set; }
    public string? NomeContato { get; set; }
    public string? Email { get; set; }
    public string? Telefone { get; set; }
    public string? Documento { get; set; }
    public string? Observacoes { get; set; }
}
