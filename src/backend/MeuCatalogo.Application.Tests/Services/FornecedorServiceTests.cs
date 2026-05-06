using FluentAssertions;
using MeuCatalogo.Application.DTOs.Requests;
using MeuCatalogo.Application.Entities;
using MeuCatalogo.Application.Services;
using MeuCatalogo.Application.Tests.Helpers;
using Xunit;

namespace MeuCatalogo.Application.Tests.Services;

public class FornecedorServiceTests
{
    private const string UserId = "user-1";

    [Fact]
    public async Task CreateAsync_PersisteFornecedorVinculadoAoUser()
    {
        await using var test = new TestDbContext();
        var service = new FornecedorService(test.Db);
        var request = new FornecedorRequest { Nome = "ACME", Email = "a@x.com" };

        var result = await service.CreateAsync(request, UserId);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Nome.Should().Be("ACME");
        var persistido = test.Db.Fornecedores.Single();
        persistido.UserId.Should().Be(UserId);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateAsync_RetornaErro_QuandoNomeVazio(string nome)
    {
        await using var test = new TestDbContext();
        var service = new FornecedorService(test.Db);
        var request = new FornecedorRequest { Nome = nome };

        var result = await service.CreateAsync(request, UserId);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task GetAllAsync_RetornaApenasDoUser()
    {
        await using var test = new TestDbContext();
        test.Db.Fornecedores.AddRange(
            new Fornecedor { Nome = "A", UserId = UserId, Ativo = true },
            new Fornecedor { Nome = "B", UserId = "outro", Ativo = true });
        await test.Db.SaveChangesAsync();

        var service = new FornecedorService(test.Db);
        var result = await service.GetAllAsync(UserId);

        result.Data.Should().HaveCount(1);
        result.Data![0].Nome.Should().Be("A");
    }

    [Fact]
    public async Task DeleteAsync_RetornaErro_QuandoFornecedorEhDeOutroUser()
    {
        await using var test = new TestDbContext();
        var fornecedor = new Fornecedor { Nome = "X", UserId = "outro", Ativo = true };
        test.Db.Fornecedores.Add(fornecedor);
        await test.Db.SaveChangesAsync();

        var service = new FornecedorService(test.Db);
        var result = await service.DeleteAsync(fornecedor.Id, UserId);

        result.IsSuccess.Should().BeFalse();
    }
}
