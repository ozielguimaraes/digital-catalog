using System;

namespace MeuCatalogo.Application.Entities;

public class TipoVariacao : BaseEntity
{
    public string Nome { get; set; }

    public TipoVariacao()
    {
    }

    public TipoVariacao(string nome) : this()
    {
        Nome = nome;
    }
}