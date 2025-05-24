using MeuCatalogo.Application.DTOs;
using MeuCatalogo.Application.Entities;

namespace MeuCatalogo.Application.Infrastructure.Mappers;

public static class CatalogoMapper
{
    public static CatalogoDto MapToDto(this Catalogo catalogo)
    {
        return new CatalogoDto
        {
            Id = catalogo.Id,
            Nome = catalogo.Nome,
            Descricao = catalogo.Descricao,
            DataCriacao = catalogo.DataCriacao,
            DataAtualizacao = catalogo.DataAtualizacao
        };
    }
}

public static class OpcaoVariacaoMapper
{
    public static OpcaoVariacaoDto MapToDto(this OpcaoVariacao opcaoVariacao)
    {
        return new OpcaoVariacaoDto
        {
            Id = opcaoVariacao.Id,
            Valor = opcaoVariacao.Valor,
        };
    }
}
