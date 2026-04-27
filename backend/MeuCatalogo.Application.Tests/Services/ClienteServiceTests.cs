using FluentAssertions;
using MeuCatalogo.Application.DTOs.Requests;
using MeuCatalogo.Application.Entities;
using MeuCatalogo.Application.Services;
using MeuCatalogo.Application.Tests.Helpers;
using Xunit;

namespace MeuCatalogo.Application.Tests.Services;

public class ClienteServiceTests
{
    private static ClienteRequest NovoRequest(string email = "novo@x.com") => new()
    {
        Nome = "Novo Cliente",
        Email = email,
        Telefone = "11999",
        InformacoesAdicionais = "obs"
    };

    [Fact]
    public async Task CreateAsync_PersisteCliente()
    {
        await using var test = new TestDbContext();
        var service = new ClienteService(test.Db);

        var result = await service.CreateAsync(NovoRequest());

        result.IsSuccess.Should().BeTrue();
        result.Data!.Email.Should().Be("novo@x.com");
        test.Db.Clientes.Should().HaveCount(1);
    }

    [Fact]
    public async Task CreateAsync_RetornaErro_QuandoEmailDuplicado()
    {
        await using var test = new TestDbContext();
        var service = new ClienteService(test.Db);
        await service.CreateAsync(NovoRequest("dup@x.com"));

        var result = await service.CreateAsync(NovoRequest("dup@x.com"));

        result.IsSuccess.Should().BeFalse();
        result.Message.Should().ContainEquivalentOf("já existe");
    }

    [Fact]
    public async Task DeleteAsync_RetornaErro_QuandoClientePossuiPedidos()
    {
        await using var test = new TestDbContext();
        var cliente = new Cliente("Fulano", "f@x.com", "9", "rua") { InformacoesAdicionais = "x" };
        test.Db.Clientes.Add(cliente);
        test.Db.Pedidos.Add(new Pedido(cliente.Id));
        await test.Db.SaveChangesAsync();

        var service = new ClienteService(test.Db);
        var result = await service.DeleteAsync(cliente.Id);

        result.IsSuccess.Should().BeFalse();
        test.Db.Clientes.Should().HaveCount(1);
    }

    [Fact]
    public async Task DeleteAsync_RemoveCliente_QuandoSemPedidos()
    {
        await using var test = new TestDbContext();
        var cliente = new Cliente("F", "f@x.com", "9", "rua");
        test.Db.Clientes.Add(cliente);
        await test.Db.SaveChangesAsync();

        var service = new ClienteService(test.Db);
        var result = await service.DeleteAsync(cliente.Id);

        result.IsSuccess.Should().BeTrue();
        test.Db.Clientes.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByEmailAsync_RetornaCliente_QuandoExiste()
    {
        await using var test = new TestDbContext();
        var service = new ClienteService(test.Db);
        await service.CreateAsync(NovoRequest("alvo@x.com"));

        var result = await service.GetByEmailAsync("alvo@x.com");

        result.IsSuccess.Should().BeTrue();
        result.Data!.Email.Should().Be("alvo@x.com");
    }

    [Fact]
    public async Task GetByEmailAsync_RetornaErro_QuandoNaoExiste()
    {
        await using var test = new TestDbContext();
        var service = new ClienteService(test.Db);

        var result = await service.GetByEmailAsync("nao@existe.com");

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_AtualizaCampos_QuandoEmailNaoMudou()
    {
        await using var test = new TestDbContext();
        var service = new ClienteService(test.Db);
        var created = await service.CreateAsync(NovoRequest("mesmo@x.com"));

        var result = await service.UpdateAsync(created.Data!.Id, new ClienteRequest
        {
            Nome = "Renomeado",
            Email = "mesmo@x.com",
            Telefone = "novo",
            InformacoesAdicionais = "atualizado"
        });

        result.IsSuccess.Should().BeTrue();
        result.Data!.Nome.Should().Be("Renomeado");
    }

    [Fact]
    public async Task UpdateAsync_RetornaErro_QuandoNovoEmailJaExiste()
    {
        await using var test = new TestDbContext();
        var service = new ClienteService(test.Db);
        await service.CreateAsync(NovoRequest("a@x.com"));
        var b = await service.CreateAsync(NovoRequest("b@x.com"));

        var result = await service.UpdateAsync(b.Data!.Id, new ClienteRequest
        {
            Nome = "B",
            Email = "a@x.com",
            Telefone = "9",
            InformacoesAdicionais = "x"
        });

        result.IsSuccess.Should().BeFalse();
        result.Message.Should().ContainEquivalentOf("já está em uso");
    }

    [Fact]
    public async Task GetByIdAsync_RetornaErro_QuandoNaoExiste()
    {
        await using var test = new TestDbContext();
        var service = new ClienteService(test.Db);

        var result = await service.GetByIdAsync(Guid.NewGuid());

        result.IsSuccess.Should().BeFalse();
    }
}
