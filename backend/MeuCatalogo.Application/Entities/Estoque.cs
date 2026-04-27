using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MeuCatalogo.Application.Entities;

public class Estoque : BaseEntity
{
    public Guid ProdutoId { get; set; }
    public Produto Produto { get; set; }
    public int? Quantidade { get; set; }
    public int? QuantidadeMinima { get; set; }
    public int? QuantidadeMaxima { get; set; }

    public bool Disponivel { get; set; }

    public Estoque()
    {
        Disponivel = true;
    }

    public Estoque(Guid produtoId, int quantidade, int? quantidadeMinima = null, int? quantidadeMaxima = null) : this()
    {
        ProdutoId = produtoId;
        Quantidade = quantidade;
        QuantidadeMinima = quantidadeMinima;
        QuantidadeMaxima = quantidadeMaxima;
    }

    public bool EhIlimitado() => Quantidade is null;

    public bool TemEstoqueSuficiente(int quantidadeSolicitada)
    {
        return EhIlimitado() || Quantidade >= quantidadeSolicitada;
    }

    public void DecrementarEstoque(int quantidade)
    {
        if (TemEstoqueSuficiente(quantidade))
        {
            Quantidade -= quantidade;
        }
        else
        {
            throw new InvalidOperationException("Estoque insuficiente");
        }
    }

    public void IncrementarEstoque(int quantidade)
    {
        Quantidade += quantidade;
    }
}
