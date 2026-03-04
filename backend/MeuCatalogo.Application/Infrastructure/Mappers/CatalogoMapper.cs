using MeuCatalogo.Application.DTOs;
using MeuCatalogo.Application.Entities;
using MeuCatalogo.Application.Interfaces;
using System.Linq;

namespace MeuCatalogo.Application.Infrastructure.Mappers;

public static class CatalogoMapper
{
    public static CatalogoDto MapToDto(this Catalogo catalogo, IStorageService storage = null)
    {
        return new CatalogoDto
        {
            Id = catalogo.Id,
            Nome = catalogo.Nome,
            NomeCurto = catalogo.NomeCurto,
            Email = catalogo.Email,
            NumeroWhatsapp = catalogo.NumeroWhatsapp,
            Descricao = catalogo.Descricao,
            DataCriacao = catalogo.DataCriacao,
            DataAtualizacao = catalogo.DataAtualizacao,
            Produtos = catalogo.Produtos?.Select(p => p.MapToDto(storage)).ToList()
        };
    }
}

public static class ProdutoMapper
{
    public static ProdutoDto MapToDto(this Produto produto, IStorageService storage = null)
    {
        return new ProdutoDto
        {
            Id = produto.Id,
            Nome = produto.Nome,
            Preco = produto.Preco,
            PrecoComDesconto = produto.PrecoComDesconto,
            InformacoesAdicionais = produto.InformacoesAdicionais,
            CategoriaId = produto.CategoriaId,
            CategoriaNome = produto.Categoria?.Nome ?? string.Empty,
            CatalogoId = produto.CatalogoId,
            DataCriacao = produto.DataCriacao,
            DataAtualizacao = produto.DataAtualizacao,
            Imagens = produto.Imagens?.Select(img => new ProdutoImagemDto
            {
                Id = img.Id,
                Url = storage != null 
                    ? storage.GetPresignedUrlFromPublicUrl(img.Url, TimeSpan.FromMinutes(60)) 
                    : img.Url,
                IsPrincipal = img.IsPrincipal,
                Ordem = img.Ordem
            }).OrderBy(i => i.Ordem).ToList() ?? new List<ProdutoImagemDto>(),
            Variacoes = produto.Variacoes?.Select(v => new VariacaoDto
            {
                Id = v.Id,
                ProdutoId = v.ProdutoId,
                TipoVariacaoId = v.TipoVariacaoId,
                TipoNome = v.TipoVariacao?.Nome ?? "N/A",
                OpcaoVariacaoId = v.OpcaoVariacaoId,
                Valor = v.OpcaoVariacao?.Valor ?? "N/A",
                DataCriacao = v.DataCriacao
            }).ToList() ?? new List<VariacaoDto>()
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
