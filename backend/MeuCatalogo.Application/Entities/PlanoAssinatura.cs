using System;
using System.Collections.Generic;

namespace MeuCatalogo.Application.Entities;

public class PlanoAssinatura : BaseEntity
{
    public string Nome { get; set; }
    public string Descricao { get; set; }
    public decimal Preco { get; set; }
    public int DuracaoEmMeses { get; set; }
    public int LimiteProdutos { get; set; }
    public int LimiteCatalogos { get; set; }
    public bool PermiteVariacoes { get; set; }
    public bool PermiteEstoque { get; set; }
    public bool PermiteRelatorios { get; set; }
    public bool PermiteExportacao { get; set; }
    public bool PermiteImportacao { get; set; }
    public bool PermitePersonalizacao { get; set; }
    public bool EhGratuito { get; set; }
    public ICollection<AssinaturaUsuario> AssinaturasUsuarios { get; set; }

    public PlanoAssinatura()
    {
        AssinaturasUsuarios = new List<AssinaturaUsuario>();
    }

    public PlanoAssinatura(string nome, string descricao, decimal preco, int duracaoEmMeses, 
        int limiteProdutos, int limiteCatalogos, bool permiteVariacoes, bool permiteEstoque, 
        bool permiteRelatorios, bool permiteExportacao, bool permiteImportacao, bool permitePersonalizacao, 
        bool ehGratuito) : this()
    {
        Nome = nome;
        Descricao = descricao;
        Preco = preco;
        DuracaoEmMeses = duracaoEmMeses;
        LimiteProdutos = limiteProdutos;
        LimiteCatalogos = limiteCatalogos;
        PermiteVariacoes = permiteVariacoes;
        PermiteEstoque = permiteEstoque;
        PermiteRelatorios = permiteRelatorios;
        PermiteExportacao = permiteExportacao;
        PermiteImportacao = permiteImportacao;
        PermitePersonalizacao = permitePersonalizacao;
        EhGratuito = ehGratuito;
    }
}