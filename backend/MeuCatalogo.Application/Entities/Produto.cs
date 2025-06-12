namespace MeuCatalogo.Application.Entities;

public class Produto : BaseEntity
{
    public string Nome { get; set; }

    public Guid CategoriaId { get; set; }
    public Categoria Categoria { get; set; }

    public Guid CatalogoId { get; set; }
    public Catalogo Catalogo { get; set; }

    public decimal Preco { get; set; }
    public decimal? PrecoComDesconto { get; set; }
    public string? InformacoesAdicionais { get; set; }
    public Estoque? Estoque { get; set; }
    public ICollection<Variacao> Variacoes { get; set; }
    public ICollection<ItemPedido> ItensPedido { get; set; }

    public Produto()
    {
        Variacoes = new List<Variacao>();
        ItensPedido = new List<ItemPedido>();
    }

    public Produto(string nome, Guid categoriaId, Guid catalogoId, decimal preco, decimal? precoComDesconto, string informacoesAdicionais) : this()
    {
        Nome = nome;
        CategoriaId = categoriaId;
        CatalogoId = catalogoId;
        Preco = preco;
        PrecoComDesconto = precoComDesconto;
        InformacoesAdicionais = informacoesAdicionais;
        Variacoes = new List<Variacao>();
        ItensPedido = new List<ItemPedido>();
    }

    public decimal ObterPrecoUnitario()
        => PrecoComDesconto ?? Preco;
}
