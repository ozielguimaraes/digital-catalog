using System;
using System.Collections.Generic;

namespace MeuCatalogo.Application.Entities;

public class Categoria : BaseEntity
{
    public Categoria()
    {
        Produtos = new List<Produto>();
    }

    public Categoria(string nome, string descricao, Guid catalogoId) : this()
    {
        Nome = nome;
        Descricao = descricao;
        CatalogoId = catalogoId;
        Produtos = new List<Produto>();
    }

    public string Nome { get; set; }
    public string Descricao { get; set; }

    public Guid CatalogoId { get; set; }
    public Catalogo Catalogo { get; set; }

    public ICollection<Produto> Produtos { get; set; }
}
