namespace MeuCatalogo.Application.Entities;

public class Variacao : BaseEntity
{
    public Guid ProdutoId { get; set; }
    public Produto Produto { get; set; }
    public Guid TipoVariacaoId { get; set; }
    public TipoVariacao TipoVariacao { get; set; }
    public Guid OpcaoVariacaoId { get; set; }
    public virtual OpcaoVariacao OpcaoVariacao { get; set; }

    public Variacao()
    {
    }

    public Variacao(Guid produtoId, Guid tipoVariacaoId, Guid opcaoVariacaoId) : this()
    {
        ProdutoId = produtoId;
        TipoVariacaoId = tipoVariacaoId;
        OpcaoVariacaoId = opcaoVariacaoId;
    }
}
