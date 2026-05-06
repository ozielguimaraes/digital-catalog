using FluentAssertions;
using MeuCatalogo.Application.DTOs.Requests;
using MeuCatalogo.Application.DTOs.Responses;
using MeuCatalogo.Application.Entities;
using MeuCatalogo.Application.Services;
using MeuCatalogo.Application.Tests.Helpers;
using Xunit;

namespace MeuCatalogo.Application.Tests.Services;

public class CategoriaFinanceiraServiceTests
{
    private const string UserId = "user-1";
    private const string OutroUserId = "user-2";

    private static CategoriaFinanceiraRequest Req(
        string nome = "Alimentação",
        CategoriaFinanceiraTipo tipo = CategoriaFinanceiraTipo.Despesa,
        string icone = "food",
        string cor = "#FF5722") => new()
    {
        Nome = nome,
        Tipo = tipo,
        IconeNome = icone,
        Cor = cor
    };

    [Fact]
    public async Task CreateAsync_PersisteCategoria()
    {
        await using var test = new TestDbContext();
        var service = new CategoriaFinanceiraService(test.Db);

        var result = await service.CreateAsync(Req(), UserId);

        result.IsSuccess.Should().BeTrue();
        result.Type.Should().Be(ResponseType.Created);
        var c = test.Db.CategoriasFinanceiras.Single();
        c.Nome.Should().Be("Alimentação");
        c.IconeNome.Should().Be("food");
        c.Cor.Should().Be("#FF5722");
        c.Tipo.Should().Be(CategoriaFinanceiraTipo.Despesa);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateAsync_NomeVazio_RetornaErro(string nome)
    {
        await using var test = new TestDbContext();
        var service = new CategoriaFinanceiraService(test.Db);

        var result = await service.CreateAsync(Req(nome: nome), UserId);

        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Nome é obrigatório");
    }

    [Fact]
    public async Task GetAllAsync_FiltraPorTipo()
    {
        await using var test = new TestDbContext();
        var service = new CategoriaFinanceiraService(test.Db);
        await service.CreateAsync(Req(nome: "Salário", tipo: CategoriaFinanceiraTipo.Receita), UserId);
        await service.CreateAsync(Req(nome: "Aluguel", tipo: CategoriaFinanceiraTipo.Despesa), UserId);
        await service.CreateAsync(Req(nome: "Outro", tipo: CategoriaFinanceiraTipo.Despesa), OutroUserId);

        var receitas = await service.GetAllAsync(UserId, CategoriaFinanceiraTipo.Receita);
        var despesas = await service.GetAllAsync(UserId, CategoriaFinanceiraTipo.Despesa);

        receitas.Data.Should().HaveCount(1);
        receitas.Data![0].Nome.Should().Be("Salário");
        despesas.Data.Should().HaveCount(1);
        despesas.Data![0].Nome.Should().Be("Aluguel");
    }

    [Fact]
    public async Task CreateSubcategoria_VinculaACategoria()
    {
        await using var test = new TestDbContext();
        var service = new CategoriaFinanceiraService(test.Db);
        var cat = await service.CreateAsync(Req(), UserId);

        var sub = await service.CreateSubcategoriaAsync(cat.Data!.Id, new SubcategoriaFinanceiraRequest { Nome = "Mercado" }, UserId);

        sub.IsSuccess.Should().BeTrue();
        sub.Data!.Nome.Should().Be("Mercado");

        var detalhe = await service.GetByIdAsync(cat.Data.Id, UserId);
        detalhe.Data!.Subcategorias.Should().HaveCount(1);
    }

    [Fact]
    public async Task CreateSubcategoria_DeOutroUser_RetornaNotFound()
    {
        await using var test = new TestDbContext();
        var service = new CategoriaFinanceiraService(test.Db);
        var cat = await service.CreateAsync(Req(), OutroUserId);

        var sub = await service.CreateSubcategoriaAsync(cat.Data!.Id, new SubcategoriaFinanceiraRequest { Nome = "x" }, UserId);

        sub.IsSuccess.Should().BeFalse();
        sub.Type.Should().Be(ResponseType.NotFound);
    }

    [Fact]
    public async Task DeleteAsync_ComLancamentoVinculado_ApenasInativa()
    {
        await using var test = new TestDbContext();
        var service = new CategoriaFinanceiraService(test.Db);
        var cat = await service.CreateAsync(Req(), UserId);
        test.Db.Lancamentos.Add(new Lancamento
        {
            UserId = UserId,
            CategoriaFinanceiraId = cat.Data!.Id,
            Descricao = "x",
            Valor = 10m,
            DataVencimento = DateTime.UtcNow,
            Tipo = LancamentoTipo.Pagar
        });
        await test.Db.SaveChangesAsync();

        var result = await service.DeleteAsync(cat.Data.Id, UserId);

        result.IsSuccess.Should().BeTrue();
        test.Db.CategoriasFinanceiras.Single().Ativo.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_SemUso_RemoveFisicamente()
    {
        await using var test = new TestDbContext();
        var service = new CategoriaFinanceiraService(test.Db);
        var cat = await service.CreateAsync(Req(), UserId);

        var result = await service.DeleteAsync(cat.Data!.Id, UserId);

        result.IsSuccess.Should().BeTrue();
        test.Db.CategoriasFinanceiras.Should().BeEmpty();
    }
}
