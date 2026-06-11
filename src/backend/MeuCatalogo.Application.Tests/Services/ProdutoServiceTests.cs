using FluentAssertions;
using MeuCatalogo.Application.DTOs;
using MeuCatalogo.Application.DTOs.Responses;
using MeuCatalogo.Application.Entities;
using MeuCatalogo.Application.Services;
using MeuCatalogo.Application.Tests.Helpers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace MeuCatalogo.Application.Tests.Services;

public class ProdutoServiceTests
{
    private static ProdutoService NewService(TestDbContext test) =>
        new(test.Db, NullLogger<ProdutoService>.Instance, new StubStorageService(), new MemoryCache(new MemoryCacheOptions()));

    [Fact]
    public async Task ObterPorIdAsync_RetornaNotFound_QuandoNaoExiste()
    {
        await using var test = new TestDbContext();
        var service = NewService(test);

        var result = await service.ObterPorIdAsync(Guid.NewGuid(), "user-1");

        result.IsSuccess.Should().BeFalse();
        result.Type.Should().Be(ResponseType.NotFound);
    }

    [Fact]
    public async Task ObterPorIdAsync_RetornaForbidden_QuandoCatalogoEhDeOutroUser()
    {
        await using var test = new TestDbContext();
        var catalogo = new Catalogo("c", "d", "outro-user", "c", "11999", "x@x.com");
        var categoria = new Categoria("cat", "desc", catalogo.Id);
        var produto = new Produto("prod", categoria.Id, catalogo.Id, 10m, null, "info");
        test.Db.AddRange(catalogo, categoria, produto);
        await test.Db.SaveChangesAsync();

        var service = NewService(test);
        var result = await service.ObterPorIdAsync(produto.Id, "user-1");

        result.IsSuccess.Should().BeFalse();
        result.Type.Should().Be(ResponseType.Forbidden);
    }

    [Fact]
    public async Task ObterPorIdAsync_RetornaProduto_QuandoCatalogoEhDoUser()
    {
        await using var test = new TestDbContext();
        var catalogo = new Catalogo("c", "d", "user-1", "c", "11999", "x@x.com");
        var categoria = new Categoria("cat", "desc", catalogo.Id);
        var produto = new Produto("prod", categoria.Id, catalogo.Id, 10m, null, "info");
        test.Db.AddRange(catalogo, categoria, produto);
        await test.Db.SaveChangesAsync();

        var service = NewService(test);
        var result = await service.ObterPorIdAsync(produto.Id, "user-1");

        result.IsSuccess.Should().BeTrue();
        result.Data!.Nome.Should().Be("prod");
    }

    [Fact]
    public async Task RemoverAsync_RetornaNotFound_QuandoNaoExiste()
    {
        await using var test = new TestDbContext();
        var service = NewService(test);

        var result = await service.RemoverAsync(Guid.NewGuid(), "user-1");

        result.IsSuccess.Should().BeFalse();
        result.Type.Should().Be(ResponseType.NotFound);
    }

    [Fact]
    public async Task ExisteAsync_RetornaTrue_QuandoProdutoEhDoUser()
    {
        await using var test = new TestDbContext();
        var catalogo = new Catalogo("c", "d", "user-1", "c", "11999", "x@x.com");
        var categoria = new Categoria("cat", "desc", catalogo.Id);
        var produto = new Produto("prod", categoria.Id, catalogo.Id, 10m, null, "info");
        test.Db.AddRange(catalogo, categoria, produto);
        await test.Db.SaveChangesAsync();

        var service = NewService(test);
        var result = await service.ExisteAsync(produto.Id, "user-1");

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeTrue();
    }

    [Fact]
    public async Task AtualizarAsync_PersisteVariacoes_ComPrecoEQuantidade()
    {
        await using var test = new TestDbContext();
        var catalogo = new Catalogo("c", "d", "user-1", "c", "11999", "x@x.com");
        var categoria = new Categoria("cat", "desc", catalogo.Id);
        var produto = new Produto("prod", categoria.Id, catalogo.Id, 10m, null, "info");
        test.Db.AddRange(catalogo, categoria, produto);
        await test.Db.SaveChangesAsync();
        test.Db.ChangeTracker.Clear(); // simula request novo (contexto sem entidades rastreadas)

        var dto = new ProdutoUpdateDto
        {
            Nome = "prod",
            CategoriaId = categoria.Id,
            Preco = 10m,
            Variacoes = new List<ProdutoVariacaoCreateDto>
            {
                new() { Cor = "Vermelho", Tamanho = "P", Preco = null, Quantidade = 5 },
                new() { Cor = "Azul", Tamanho = "M", Preco = 12m, Quantidade = 3 }
            }
        };

        var service = NewService(test);
        var result = await service.AtualizarAsync(produto.Id, dto, "user-1");

        result.IsSuccess.Should().BeTrue();
        result.Data!.Variacoes.Should().HaveCount(2);

        var recarregado = await service.ObterPorIdAsync(produto.Id, "user-1");
        recarregado.Data!.Variacoes.Should().HaveCount(2);
        recarregado.Data.Variacoes.Should().ContainSingle(v =>
            v.Cor == "Vermelho" && v.Tamanho == "P" && v.Preco == null && v.Quantidade == 5);
        recarregado.Data.Variacoes.Should().ContainSingle(v =>
            v.Cor == "Azul" && v.Tamanho == "M" && v.Preco == 12m && v.Quantidade == 3);
    }

    [Fact]
    public async Task AtualizarAsync_SubstituiVariacoesExistentes()
    {
        await using var test = new TestDbContext();
        var catalogo = new Catalogo("c", "d", "user-1", "c", "11999", "x@x.com");
        var categoria = new Categoria("cat", "desc", catalogo.Id);
        var produto = new Produto("prod", categoria.Id, catalogo.Id, 10m, null, "info");
        var variacaoAntiga = new ProdutoVariacao { ProdutoId = produto.Id, Cor = "Preto", Tamanho = "G", Quantidade = 1 };
        test.Db.AddRange(catalogo, categoria, produto, variacaoAntiga);
        await test.Db.SaveChangesAsync();
        test.Db.ChangeTracker.Clear(); // simula request novo (contexto sem entidades rastreadas)

        var dto = new ProdutoUpdateDto
        {
            Nome = "prod",
            CategoriaId = categoria.Id,
            Preco = 10m,
            Variacoes = new List<ProdutoVariacaoCreateDto>
            {
                new() { Cor = "Branco", Tamanho = "PP", Quantidade = 7 }
            }
        };

        var service = NewService(test);
        var result = await service.AtualizarAsync(produto.Id, dto, "user-1");

        result.IsSuccess.Should().BeTrue();
        var recarregado = await service.ObterPorIdAsync(produto.Id, "user-1");
        recarregado.Data!.Variacoes.Should().ContainSingle()
            .Which.Cor.Should().Be("Branco");
    }

    [Fact]
    public async Task ObterSugestoesVariacaoAsync_RetornaCoresETamanhosDistintos()
    {
        await using var test = new TestDbContext();
        var catalogo = new Catalogo("c", "d", "user-1", "c", "11999", "x@x.com");
        var categoria = new Categoria("cat", "desc", catalogo.Id);
        var produto = new Produto("prod", categoria.Id, catalogo.Id, 10m, null, "info");
        test.Db.AddRange(catalogo, categoria, produto,
            new ProdutoVariacao { ProdutoId = produto.Id, Cor = "Vermelho", Tamanho = "P", Quantidade = 1 },
            new ProdutoVariacao { ProdutoId = produto.Id, Cor = "Vermelho", Tamanho = "M", Quantidade = 1 },
            new ProdutoVariacao { ProdutoId = produto.Id, Cor = "Azul", Tamanho = "P", Quantidade = 1 });
        await test.Db.SaveChangesAsync();

        var service = NewService(test);
        var result = await service.ObterSugestoesVariacaoAsync(catalogo.Id, "user-1");

        result.IsSuccess.Should().BeTrue();
        result.Data!.Cores.Should().Equal("Azul", "Vermelho");
        result.Data.Tamanhos.Should().Equal("M", "P");
    }

    [Fact]
    public async Task ObterSugestoesVariacaoAsync_RetornaForbidden_QuandoCatalogoEhDeOutroUser()
    {
        await using var test = new TestDbContext();
        var catalogo = new Catalogo("c", "d", "outro-user", "c", "11999", "x@x.com");
        test.Db.Add(catalogo);
        await test.Db.SaveChangesAsync();

        var service = NewService(test);
        var result = await service.ObterSugestoesVariacaoAsync(catalogo.Id, "user-1");

        result.IsSuccess.Should().BeFalse();
        result.Type.Should().Be(ResponseType.Forbidden);
    }
}
