using MeuCatalogo.Application.DTOs.Responses;
using MeuCatalogo.Application.Entities;

namespace MeuCatalogo.Application.Services;

internal static class LancamentoMapper
{
    public static LancamentoResponse MapToResponse(Lancamento l)
    {
        var valorBaixado = l.Baixas?.Where(b => b.Ativo).Sum(b => b.Valor) ?? 0m;
        return new LancamentoResponse
        {
            Id = l.Id,
            Descricao = l.Descricao,
            Valor = l.Valor,
            DataVencimento = l.DataVencimento,
            DataPagamento = l.DataPagamento,
            Tipo = l.Tipo,
            Status = l.Status,
            Observacoes = l.Observacoes,
            PedidoId = l.PedidoId,
            FornecedorId = l.FornecedorId,
            FornecedorNome = l.Fornecedor?.Nome,
            CreatedAt = l.DataCriacao,
            UpdatedAt = l.DataAtualizacao,
            ContaId = l.ContaId,
            ContaNome = l.Conta?.Nome,
            CategoriaFinanceiraId = l.CategoriaFinanceiraId,
            CategoriaFinanceiraNome = l.CategoriaFinanceira?.Nome,
            CategoriaFinanceiraIcone = l.CategoriaFinanceira?.IconeNome,
            CategoriaFinanceiraCor = l.CategoriaFinanceira?.Cor,
            SubcategoriaFinanceiraId = l.SubcategoriaFinanceiraId,
            SubcategoriaFinanceiraNome = l.SubcategoriaFinanceira?.Nome,
            Operacao = l.Operacao,
            TipoTransferencia = l.TipoTransferencia,
            LancamentoTransferenciaId = l.LancamentoTransferenciaId,
            ParcelaAtual = l.ParcelaAtual,
            ParcelaTotal = l.ParcelaTotal,
            FaturaId = l.FaturaId,
            RecorrenciaId = l.RecorrenciaId,
            ComprovanteFinanceiroId = l.ComprovanteFinanceiroId,
            Realizado = l.Realizado,
            ValorBaixado = valorBaixado,
            ValorEmAberto = Math.Max(0m, l.Valor - valorBaixado)
        };
    }
}
