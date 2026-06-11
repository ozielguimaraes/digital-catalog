using System;

namespace MeuCatalogo.Application.Entities;

public class ProdutoVariacao : BaseEntity
{
    public Guid ProdutoId { get; set; }
    public Produto Produto { get; set; }
    public string? Cor { get; set; }
    public string? Tamanho { get; set; }
    public decimal? Preco { get; set; }
    public int Quantidade { get; set; }

    public ProdutoVariacao()
    {
    }

    public ProdutoVariacao(Guid produtoId, string? cor, string? tamanho, decimal? preco, int quantidade) : this()
    {
        ProdutoId = produtoId;
        Cor = cor;
        Tamanho = tamanho;
        Preco = preco;
        Quantidade = quantidade;
    }
}
