using System;
using System.Collections.Generic;

namespace MeuCatalogo.Application.Entities;

public class Catalogo : BaseEntity
{
    public string Nome { get; set; }
    public string Descricao { get; set; }
    public string UserId { get; set; }
    public ApplicationUser User { get; set; }
    public ICollection<Produto> Produtos { get; set; }

    public Catalogo()
    {
        Produtos = new List<Produto>();
    }

    public Catalogo(string nome, string descricao, string userId) : this()
    {
        Nome = nome;
        Descricao = descricao;
        UserId = userId;
        Produtos = new List<Produto>();
    }
}