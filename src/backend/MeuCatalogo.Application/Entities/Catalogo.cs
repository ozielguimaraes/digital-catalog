using System;
using System.Collections.Generic;

namespace MeuCatalogo.Application.Entities;

public class Catalogo : BaseEntity
{
    public string Nome { get; set; }
    public string Descricao { get; set; }
    public string NomeCurto { get; set; }
    public string NumeroWhatsapp { get; set; }
    public string Email { get; set; }
    public string UserId { get; set; }
    public ApplicationUser User { get; set; }
    public ICollection<Produto> Produtos { get; set; }

    public Catalogo()
    {
        Produtos = new List<Produto>();
    }

    public Catalogo(string nome, string descricao, string userId, string nomeCurto, string numeroWhatsapp, string email) : this()
    {
        DataCriacao = DateTime.UtcNow;
        Nome = nome;
        Descricao = descricao;
        UserId = userId;
        NomeCurto = nomeCurto;
        NumeroWhatsapp = numeroWhatsapp;
        Email = email;
        Produtos = new List<Produto>();
    }
}
