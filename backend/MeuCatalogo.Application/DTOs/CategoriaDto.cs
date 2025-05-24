using System;

namespace MeuCatalogo.Application.DTOs;

public class CategoriaDto
{
    public Guid Id { get; set; }
    public string Nome { get; set; }
    public string Descricao { get; set; }
    public Guid CatalogoId { get; set; }
    public DateTime DataCriacao { get; set; }
    public DateTime? DataAtualizacao { get; set; }
}

public class CategoriaCreateDto
{
    public string Nome { get; set; }
    public string Descricao { get; set; }
    public Guid CatalogoId { get; set; }
}

public class CategoriaUpdateDto
{
    public string Nome { get; set; }
    public string Descricao { get; set; }
}