using System;
using System.Collections.Generic;

namespace MeuCatalogo.Application.Entities;

public class Cliente : BaseEntity
{
    public string Nome { get; set; }
    public string Email { get; set; }
    public string Telefone { get; set; }
    public string Endereco { get; set; }
    public ICollection<Pedido> Pedidos { get; set; }
    public string InformacoesAdicionais { get; set; }

    public Cliente()
    {
        Pedidos = new List<Pedido>();
    }

    public Cliente(string nome, string email, string telefone, string endereco) : this()
    {
        Nome = nome;
        Email = email;
        Telefone = telefone;
        Endereco = endereco;
        Pedidos = new List<Pedido>();
    }
}
