namespace MeuCatalogo.Features.Financeiro.Domain;

public enum LancamentoTipo { Receber, Pagar }
public enum LancamentoStatus { Pendente, Pago, Atrasado, Cancelado, Parcial }
public enum LancamentoOperacao { Simples, Transferencia }
public enum LancamentoTipoTransferencia { EntreContas, PagamentoFatura }
public enum ContaTipo { Carteira, ContaCorrente, CartaoCredito, Poupanca, ContaPagamento, CarteiraDigital, CartaoBeneficio, Outros }
public enum CategoriaFinanceiraTipo { Receita, Despesa }
public enum RecorrenciaPeriodicidade { Mensal, Semanal, Anual }
public enum RegimeContabil { Competencia, Caixa }

public sealed record LancamentoInfo
{
    public required Guid Id { get; init; }
    public required string Descricao { get; init; }
    public decimal Valor { get; init; }
    public DateTime DataVencimento { get; init; }
    public DateTime? DataPagamento { get; init; }
    public LancamentoTipo Tipo { get; init; }
    public LancamentoStatus Status { get; init; }
    public string? FornecedorNome { get; init; }
    public Guid? PedidoId { get; init; }
    public Guid? ContaId { get; init; }
    public string? ContaNome { get; init; }
    public Guid? CategoriaFinanceiraId { get; init; }
    public string? CategoriaFinanceiraNome { get; init; }
    public string? CategoriaFinanceiraIcone { get; init; }
    public string? CategoriaFinanceiraCor { get; init; }
    public Guid? SubcategoriaFinanceiraId { get; init; }
    public string? SubcategoriaFinanceiraNome { get; init; }
    public LancamentoOperacao Operacao { get; init; }
    public short? ParcelaAtual { get; init; }
    public short? ParcelaTotal { get; init; }
    public Guid? FaturaId { get; init; }
    public Guid? RecorrenciaId { get; init; }
    public bool Realizado { get; init; }
    public decimal ValorBaixado { get; init; }
    public decimal ValorEmAberto { get; init; }

    public string VencimentoLabel
    {
        get
        {
            var local = DataVencimento.ToLocalTime();
            return Status switch
            {
                LancamentoStatus.Pago when DataPagamento.HasValue => $"pago em {DataPagamento.Value.ToLocalTime():dd MMM}".ToLowerInvariant(),
                LancamentoStatus.Atrasado => $"vencido {local:dd MMM}".ToLowerInvariant(),
                LancamentoStatus.Parcial => $"parcial · {ValorEmAberto:C}".ToLowerInvariant(),
                _ => $"vence {local:dd MMM}".ToLowerInvariant()
            };
        }
    }

    public string ParcelaLabel => ParcelaAtual.HasValue && ParcelaTotal.HasValue && ParcelaTotal > 1
        ? $"{ParcelaAtual}/{ParcelaTotal}"
        : string.Empty;
}

public sealed record FinanceiroResumoInfo
{
    public decimal TotalAReceber { get; init; }
    public decimal TotalAPagar { get; init; }
    public decimal SaldoPrevisto { get; init; }
    public decimal RecebidoNoMes { get; init; }
    public decimal PagoNoMes { get; init; }
    public string PeriodoLabel { get; init; } = string.Empty;
}

public sealed record ContaInfo
{
    public required Guid Id { get; init; }
    public required string Nome { get; init; }
    public ContaTipo Tipo { get; init; }
    public string Cor { get; init; } = string.Empty;
    public byte? Ordem { get; init; }
    public decimal? Limite { get; init; }
    public byte? DiaFechamento { get; init; }
    public byte? DiaVencimento { get; init; }
    public decimal SaldoInicial { get; init; }
    public bool Ativo { get; init; }

    public string TipoLabel => Tipo switch
    {
        ContaTipo.Carteira => "Carteira",
        ContaTipo.ContaCorrente => "Conta Corrente",
        ContaTipo.CartaoCredito => "Cartão de Crédito",
        ContaTipo.Poupanca => "Poupança",
        ContaTipo.ContaPagamento => "Conta de Pagamento",
        ContaTipo.CarteiraDigital => "Carteira Digital",
        ContaTipo.CartaoBeneficio => "Cartão Benefício",
        _ => "Outros"
    };

    public bool EhCartaoCredito => Tipo == ContaTipo.CartaoCredito;
}

public sealed record CategoriaFinanceiraInfo
{
    public required Guid Id { get; init; }
    public CategoriaFinanceiraTipo Tipo { get; init; }
    public required string Nome { get; init; }
    public string IconeNome { get; init; } = "tag";
    public string Cor { get; init; } = "#9E9E9E";
    public byte? Ordem { get; init; }
    public bool Ativo { get; init; }
    public List<SubcategoriaFinanceiraInfo> Subcategorias { get; init; } = new();
}

public sealed record SubcategoriaFinanceiraInfo
{
    public required Guid Id { get; init; }
    public Guid CategoriaFinanceiraId { get; init; }
    public required string Nome { get; init; }
    public string? IconeNome { get; init; }
    public string? Cor { get; init; }
    public byte? Ordem { get; init; }
    public bool Ativo { get; init; }
}

public sealed record FaturaInfo
{
    public required Guid Id { get; init; }
    public Guid ContaId { get; init; }
    public string ContaNome { get; init; } = string.Empty;
    public int Mes { get; init; }
    public int Ano { get; init; }
    public DateTime DataInicio { get; init; }
    public DateTime DataFim { get; init; }
    public DateTime DataVencimento { get; init; }
    public decimal ValorTotal { get; init; }
    public decimal? ValorPago { get; init; }
    public decimal ValorEmAberto { get; init; }
    public bool Quitada { get; init; }
    public List<LancamentoInfo> Lancamentos { get; init; } = new();
}

public sealed record RecorrenciaInfo
{
    public required Guid Id { get; init; }
    public CategoriaFinanceiraTipo Tipo { get; init; }
    public required string Descricao { get; init; }
    public Guid ContaId { get; init; }
    public string? ContaNome { get; init; }
    public Guid CategoriaFinanceiraId { get; init; }
    public string? CategoriaFinanceiraNome { get; init; }
    public Guid? SubcategoriaFinanceiraId { get; init; }
    public string? SubcategoriaFinanceiraNome { get; init; }
    public decimal? ValorPadrao { get; init; }
    public RecorrenciaPeriodicidade Periodicidade { get; init; }
    public byte? DiaDoMes { get; init; }
    public DayOfWeek? DiaDaSemana { get; init; }
    public DateTime DataInicio { get; init; }
    public DateTime? DataFim { get; init; }
    public DateTime ProximaData { get; init; }
    public bool Ativo { get; init; }

    public string ValorLabel => ValorPadrao.HasValue ? $"{ValorPadrao:C}" : "Variável";
    public string PeriodicidadeLabel => Periodicidade switch
    {
        RecorrenciaPeriodicidade.Mensal => DiaDoMes.HasValue ? $"Todo dia {DiaDoMes}" : "Mensal",
        RecorrenciaPeriodicidade.Semanal => "Semanal",
        RecorrenciaPeriodicidade.Anual => "Anual",
        _ => Periodicidade.ToString()
    };
}

public sealed record LancamentoBaixaInfo
{
    public required Guid Id { get; init; }
    public Guid LancamentoId { get; init; }
    public DateTime Data { get; init; }
    public decimal Valor { get; init; }
    public Guid ContaId { get; init; }
    public string? ContaNome { get; init; }
    public Guid? ComprovanteFinanceiroId { get; init; }
    public string? Observacoes { get; init; }
}

public sealed record ComprovanteFinanceiroInfo
{
    public required Guid Id { get; init; }
    public string? Descricao { get; init; }
    public string FileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
    public long Size { get; init; }
    public string Url { get; init; } = string.Empty;
}

public sealed record RelatorioFinanceiroInfo
{
    public int Ano { get; init; }
    public int Mes { get; init; }
    public int Quantidade { get; init; }
    public List<RelatorioMesInfo> Meses { get; init; } = new();
    public List<RelatorioCategoriaInfo> Receitas { get; init; } = new();
    public List<RelatorioCategoriaInfo> Despesas { get; init; } = new();
    public decimal TotalReceitas { get; init; }
    public decimal TotalDespesas { get; init; }
    public decimal Saldo { get; init; }
}

public sealed record RelatorioMesInfo
{
    public int Ano { get; init; }
    public int Mes { get; init; }
    public string Label { get; init; } = string.Empty;
    public decimal Receitas { get; init; }
    public decimal Despesas { get; init; }
    public decimal Saldo { get; init; }
}

public sealed record RelatorioCategoriaInfo
{
    public Guid? CategoriaFinanceiraId { get; init; }
    public string Nome { get; init; } = string.Empty;
    public string? Icone { get; init; }
    public string? Cor { get; init; }
    public CategoriaFinanceiraTipo Tipo { get; init; }
    public decimal Total { get; init; }
}

public enum ExtratoMovimentoTipo { Entrada, Saida }
public enum ExtratoMovimentoOrigem { Lancamento, Baixa }

public sealed record ExtratoInfo
{
    public DateTime PeriodoInicio { get; init; }
    public DateTime PeriodoFim { get; init; }
    public List<Guid> ContaIds { get; init; } = new();
    public decimal SaldoInicial { get; init; }
    public decimal SaldoFinal { get; init; }
    public decimal TotalEntradas { get; init; }
    public decimal TotalSaidas { get; init; }
    public List<ExtratoMovimentoInfo> Movimentos { get; init; } = new();
    public List<ExtratoSaldoDiarioInfo> SaldosDiarios { get; init; } = new();
}

public sealed record ExtratoMovimentoInfo
{
    public required Guid Id { get; init; }
    public ExtratoMovimentoOrigem Origem { get; init; }
    public Guid LancamentoId { get; init; }
    public Guid ContaId { get; init; }
    public string ContaNome { get; init; } = string.Empty;
    public DateTime Data { get; init; }
    public string Descricao { get; init; } = string.Empty;
    public ExtratoMovimentoTipo Tipo { get; init; }
    public decimal Valor { get; init; }
    public decimal SaldoApos { get; init; }
    public string? CategoriaNome { get; init; }
    public string? CategoriaIcone { get; init; }
    public string? CategoriaCor { get; init; }
    public string? Observacoes { get; init; }

    public string ValorComSinal => Tipo == ExtratoMovimentoTipo.Entrada ? $"+ {Valor:C}" : $"- {Valor:C}";
}

public sealed record ExtratoSaldoDiarioInfo
{
    public DateTime Data { get; init; }
    public decimal Entradas { get; init; }
    public decimal Saidas { get; init; }
    public decimal SaldoFinalDia { get; init; }
}
