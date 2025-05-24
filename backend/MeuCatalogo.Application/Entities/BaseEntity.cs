using System;

namespace MeuCatalogo.Application.Entities;

public abstract class BaseEntity
{
    public Guid Id { get; set; }
    public DateTime DataCriacao { get; set; }
    public DateTime? DataAtualizacao { get; set; }
    public bool Ativo { get; set; }

    protected BaseEntity()
    {
        Id = Guid.NewGuid();
        DataCriacao = DateTime.UtcNow;
        Ativo = true;
    }
}