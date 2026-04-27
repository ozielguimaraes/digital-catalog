using FluentAssertions;
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
}
