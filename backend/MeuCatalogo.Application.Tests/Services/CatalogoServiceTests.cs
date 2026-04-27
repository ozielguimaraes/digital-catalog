using FluentAssertions;
using MeuCatalogo.Application.DTOs.Responses;
using MeuCatalogo.Application.Entities;
using MeuCatalogo.Application.Services;
using MeuCatalogo.Application.Tests.Helpers;
using Microsoft.Extensions.Caching.Memory;
using Xunit;

namespace MeuCatalogo.Application.Tests.Services;

public class CatalogoServiceTests
{
    private static CatalogoService NewService(TestDbContext test) =>
        new(test.Db, new StubStorageService(), new MemoryCache(new MemoryCacheOptions()));

    private static Catalogo NovoCatalogo(string userId = "user-1", string nome = "cat") =>
        new(nome, "desc", userId, nome, "11999999999", "x@x.com");

    [Fact]
    public async Task ObterPorUsuarioIdAsync_RetornaApenasCatalogosDoUser()
    {
        await using var test = new TestDbContext();
        test.Db.Catalogos.AddRange(
            NovoCatalogo(userId: "user-1", nome: "a"),
            NovoCatalogo(userId: "user-1", nome: "b"),
            NovoCatalogo(userId: "user-2", nome: "c"));
        await test.Db.SaveChangesAsync();

        var service = NewService(test);
        var result = await service.ObterPorUsuarioIdAsync("user-1");

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(2);
    }

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
    public async Task ObterPorIdAsync_RetornaErro_QuandoCatalogoEhDeOutroUser()
    {
        await using var test = new TestDbContext();
        var catalogo = NovoCatalogo(userId: "outro");
        test.Db.Catalogos.Add(catalogo);
        await test.Db.SaveChangesAsync();

        var service = NewService(test);
        var result = await service.ObterPorIdAsync(catalogo.Id, "user-1");

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteCatalogoAsync_RetornaForbidden_QuandoCatalogoEhDeOutroUser()
    {
        await using var test = new TestDbContext();
        var catalogo = NovoCatalogo(userId: "outro");
        test.Db.Catalogos.Add(catalogo);
        await test.Db.SaveChangesAsync();

        var service = NewService(test);
        var result = await service.DeleteCatalogoAsync(catalogo.Id, "user-1");

        result.IsSuccess.Should().BeFalse();
        result.Type.Should().Be(ResponseType.Forbidden);
    }

    [Fact]
    public async Task DeleteCatalogoAsync_RemoveCatalogo_QuandoDoUser()
    {
        await using var test = new TestDbContext();
        var catalogo = NovoCatalogo(userId: "user-1");
        test.Db.Catalogos.Add(catalogo);
        await test.Db.SaveChangesAsync();

        var service = NewService(test);
        var result = await service.DeleteCatalogoAsync(catalogo.Id, "user-1");

        result.IsSuccess.Should().BeTrue();
        test.Db.Catalogos.Should().BeEmpty();
    }

    [Theory]
    [InlineData("Café Espresso!", "cafe-espresso-")]
    [InlineData("AÇAÍ na Tigela", "acai-na-tigela")]
    [InlineData("ABC", "abc")]
    public void NormalizarNomeCurto_RemoveAcentosEEspeciais(string input, string esperado)
    {
        CatalogoService.NormalizarNomeCurto(input.ToLower()).Should().Be(esperado);
    }

    [Fact]
    public async Task ObterPorUsuarioIdAsync_UsaCache_NaSegundaChamada()
    {
        await using var test = new TestDbContext();
        test.Db.Catalogos.Add(NovoCatalogo(userId: "user-1", nome: "a"));
        await test.Db.SaveChangesAsync();

        var service = NewService(test);
        var primeira = await service.ObterPorUsuarioIdAsync("user-1");

        // Adiciona outro DEPOIS — segunda chamada deve vir do cache, ignorando.
        test.Db.Catalogos.Add(NovoCatalogo(userId: "user-1", nome: "b"));
        await test.Db.SaveChangesAsync();

        var segunda = await service.ObterPorUsuarioIdAsync("user-1");

        primeira.Data.Should().HaveCount(1);
        segunda.Data.Should().HaveCount(1);
    }

    [Fact]
    public async Task ObterTodosPublicosAsync_RetornaCatalogosDeQualquerUser()
    {
        await using var test = new TestDbContext();
        test.Db.Catalogos.AddRange(
            NovoCatalogo(userId: "u1", nome: "a"),
            NovoCatalogo(userId: "u2", nome: "b"));
        await test.Db.SaveChangesAsync();

        var service = NewService(test);
        var result = await service.ObterTodosPublicosAsync();

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(2);
    }

    [Fact]
    public async Task ObterCatalogoIdAsync_RetornaIdDoProduto_QuandoExiste()
    {
        await using var test = new TestDbContext();
        var catalogo = NovoCatalogo(userId: "user-1");
        var categoria = new Categoria("c", "d", catalogo.Id);
        var produto = new Produto("p", categoria.Id, catalogo.Id, 10m, null, "info");
        test.Db.AddRange(catalogo, categoria, produto);
        await test.Db.SaveChangesAsync();

        var service = NewService(test);
        var result = await service.ObterCatalogoIdAsync(produto.Id, "user-1");

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Be(catalogo.Id);
    }

    [Fact]
    public async Task ObterCatalogoIdAsync_RetornaNotFound_QuandoProdutoNaoEhDoUser()
    {
        await using var test = new TestDbContext();
        var service = NewService(test);

        var result = await service.ObterCatalogoIdAsync(Guid.NewGuid(), "user-1");

        result.IsSuccess.Should().BeFalse();
        result.Type.Should().Be(ResponseType.NotFound);
    }
}
