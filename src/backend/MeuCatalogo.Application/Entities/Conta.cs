namespace MeuCatalogo.Application.Entities;

public enum ContaTipo
{
    Carteira,
    ContaCorrente,
    CartaoCredito,
    Poupanca,
    ContaPagamento,
    CarteiraDigital,
    CartaoBeneficio,
    Outros
}

public class Conta : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }

    public string Nome { get; set; } = string.Empty;
    public ContaTipo Tipo { get; set; }
    public string Cor { get; set; } = "#3F51B5";
    public byte? Ordem { get; set; }

    public decimal? Limite { get; set; }
    public byte? DiaFechamento { get; set; }
    public byte? DiaVencimento { get; set; }

    public decimal SaldoInicial { get; set; }

    public bool EhCartaoCredito() => Tipo == ContaTipo.CartaoCredito;
}
