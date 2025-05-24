namespace MeuCatalogo.Application.Entities;

public class OpcaoVariacao : BaseEntity
{
    public string Valor { get; private set; }
    public Guid TipoVariacaoId { get; private set; }
    public TipoVariacao? TipoVariacao { get; set; }

    protected OpcaoVariacao()
    {
    }

    public OpcaoVariacao(string valor, Guid tipoVariacaoId) : this()
    {
        Valor = valor;
        TipoVariacaoId = tipoVariacaoId;
    }
}
