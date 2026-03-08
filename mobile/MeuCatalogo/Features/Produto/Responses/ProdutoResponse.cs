using MeuCatalogo.Features.Produto.Models;

namespace MeuCatalogo.Features.Produto.Responses;

public class ProdutoResponse
{
    public Guid Id { get; set; }
    public string Nome { get; set; }
    public decimal Preco { get; set; }
    public decimal? PrecoComDesconto { get; set; }
    public string? InformacoesAdicionais { get; set; }
    public Guid CategoriaId { get; set; }
    public string CategoriaNome { get; set; }
    public Guid CatalogoId { get; set; }
    public EstoqueResponse? Estoque { get; set; }
    public List<ProdutoImagemResponse> Imagens { get; set; } = [];
}

public class EstoqueResponse
{
    public Guid Id { get; set; }
    public Guid ProdutoId { get; set; }
    public int? Quantidade { get; set; }
    public int? QuantidadeMinima { get; set; }
    public int? QuantidadeMaxima { get; set; }
    public bool Disponivel { get; set; }
    public bool EhIlimitado { get; set; }
}

public class ProdutoImagemResponse
{
    public Guid Id { get; set; }
    public string Url { get; set; }
    public ProdutoImagemLinksResponse Images { get; set; } = new();
    public bool IsPrincipal { get; set; }
    public int Ordem { get; set; }
}

public class ProdutoImagemLinksResponse
{
    public string Thumbnail { get; set; } = string.Empty;
    public string Medium { get; set; } = string.Empty;
    public string Full { get; set; } = string.Empty;
}
