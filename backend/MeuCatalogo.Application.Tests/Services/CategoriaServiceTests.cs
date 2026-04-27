using FluentAssertions;
using MeuCatalogo.Application.DTOs.Categoria;
using MeuCatalogo.Application.DTOs.Requests;
using MeuCatalogo.Application.DTOs.Responses;
using MeuCatalogo.Application.Entities;
using MeuCatalogo.Application.Services;
using MeuCatalogo.Application.Tests.Helpers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace MeuCatalogo.Application.Tests.Services;

public class CategoriaServiceTests
{
    private static CategoriaService NewService(TestDbContext test) =>
        new(test.Db, NullLogger<CategoriaService>.Instance, new MemoryCache(new MemoryCacheOptions()));

    [Fact]
    public async Task AdicionarAsync_PersisteCategoria()
    {
        await using var test = new TestDbContext();
        var service = NewService(test);
        var catalogoId = Guid.NewGuid();

        var result = await service.AdicionarAsync("user-1", new CategoriaRequest
        {
            Nome = "Bebidas",
            Descricao = "cerveja, refri",
            CatalogoId = catalogoId
        });

        result.IsSuccess.Should().BeTrue();
        result.Data!.Nome.Should().Be("Bebidas");
        test.Db.Categorias.Should().HaveCount(1);
    }

    [Fact]
    public async Task ObterPorIdAsync_RetornaNotFound_QuandoNaoExiste()
    {
        await using var test = new TestDbContext();
        var service = NewService(test);

        var result = await service.ObterPorIdAsync("user-1", Guid.NewGuid());

        result.IsSuccess.Should().BeFalse();
        result.Type.Should().Be(ResponseType.NotFound);
    }

    [Fact]
    public async Task AtualizarAsync_AtualizaCampos()
    {
        await using var test = new TestDbContext();
        var categoria = new Categoria("antigo", "desc", Guid.NewGuid());
        test.Db.Categorias.Add(categoria);
        await test.Db.SaveChangesAsync();

        var service = NewService(test);
        var result = await service.AtualizarAsync(categoria.Id, "user-1", new AtualizarCategoriaRequest
        {
            Nome = "novo",
            Descricao = "nova desc"
        });

        result.IsSuccess.Should().BeTrue();
        result.Data!.Nome.Should().Be("novo");
    }

    [Fact]
    public async Task RemoverAsync_RetornaErro_QuandoExistemProdutosAssociados()
    {
        await using var test = new TestDbContext();
        var catalogo = new Catalogo("c", "d", "u", "c", "11", "x@x.com");
        var categoria = new Categoria("cat", "desc", catalogo.Id);
        var produto = new Produto("p", categoria.Id, catalogo.Id, 10m, null, "info");
        test.Db.AddRange(catalogo, categoria, produto);
        await test.Db.SaveChangesAsync();

        var service = NewService(test);
        var result = await service.RemoverAsync(categoria.Id, "user-1");

        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("produtos");
    }
}
