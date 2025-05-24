using System;

namespace MeuCatalogo.Application.DTOs;

public class PlanoAssinaturaDto
{
    public Guid Id { get; set; }
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
    public DateTime DataCriacao { get; set; }
    public DateTime? DataAtualizacao { get; set; }
}

public class PlanoAssinaturaCreateDto
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
}

public class PlanoAssinaturaUpdateDto
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
}