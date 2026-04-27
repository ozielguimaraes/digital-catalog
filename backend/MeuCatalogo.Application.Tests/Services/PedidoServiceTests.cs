using FluentAssertions;
using MeuCatalogo.Application.DTOs.Requests;
using MeuCatalogo.Application.Entities;
using MeuCatalogo.Application.Services;
using MeuCatalogo.Application.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace MeuCatalogo.Application.Tests.Services;

public class PedidoServiceTests
{
    private static PedidoService NewService(TestDbContext test) =>
        new(test.Db, NullLogger<PedidoService>.Instance);

    private static Catalogo NovoCatalogo() =>
        new("cat", "desc", "owner", "cat", "11999999999", "x@x.com");

    private static Categoria NovaCategoria(Guid catalogoId) =>
        new("cat", "desc", catalogoId);

    private static Cliente NovoCliente(string nome = "Fulano", string email = "fulano@x.com") =>
        new(nome, email, "999", "rua") { InformacoesAdicionais = "obs" };

    private static async Task<(Cliente Cliente, Produto Produto, Estoque Estoque)> SeedClienteEProdutoAsync(
        TestDbContext test,
        decimal preco = 10m,
        decimal? precoComDesconto = null,
        int? quantidadeEstoque = 100,
        bool disponivel = true)
    {
        var catalogo = NovoCatalogo();
        var categoria = NovaCategoria(catalogo.Id);
        var produto = new Produto("Produto X", categoria.Id, catalogo.Id, preco, precoComDesconto, "info");
        var estoque = new Estoque(produto.Id, quantidadeEstoque ?? 0)
        {
            Disponivel = disponivel,
            Quantidade = quantidadeEstoque
        };
        var cliente = NovoCliente();

        test.Db.Catalogos.Add(catalogo);
        test.Db.Categorias.Add(categoria);
        test.Db.Produtos.Add(produto);
        test.Db.Estoques.Add(estoque);
        test.Db.Clientes.Add(cliente);
        await test.Db.SaveChangesAsync();

        if (!disponivel)
        {
            // EF 6 não envia bool=false em INSERT quando coluna tem HasDefaultValue(true).
            // Marca como modified após o insert para garantir o false no banco.
            estoque.Disponivel = false;
            await test.Db.SaveChangesAsync();
        }

        return (cliente, produto, estoque);
    }

    [Fact]
    public async Task CreateAsync_ComItemValido_CriaPedidoEDecrementaEstoque()
    {
        await using var test = new TestDbContext();
        var (cliente, produto, _) = await SeedClienteEProdutoAsync(test, preco: 25m, quantidadeEstoque: 10);

        var service = NewService(test);
        var request = new PedidoRequest
        {
            ClienteId = cliente.Id,
            Itens = new List<ItemPedidoRequest> { new() { ProdutoId = produto.Id, Quantidade = 3 } }
        };

        var result = await service.CreateAsync(request);

        result.IsSuccess.Should().BeTrue(result.Message);
        result.Data!.ClienteId.Should().Be(cliente.Id);
        result.Data.ValorTotal.Should().Be(75m);
        result.Data.Itens.Should().HaveCount(1);
        result.Data.Itens[0].Subtotal.Should().Be(75m);
        result.Data.Itens[0].ProdutoNome.Should().Be("Produto X");
        result.Data.Itens[0].VariacaoDescricao.Should().BeNull();

        var estoqueAtualizado = await test.Db.Estoques.AsNoTracking().FirstAsync();
        estoqueAtualizado.Quantidade.Should().Be(7);
    }

    [Fact]
    public async Task CreateAsync_ComVariacao_SnapshottaDescricaoDaVariacao()
    {
        await using var test = new TestDbContext();
        var (cliente, produto, _) = await SeedClienteEProdutoAsync(test);

        var tipo = new TipoVariacao("Cor");
        var opcao = new OpcaoVariacao("Vermelho", tipo.Id);
        var variacao = new Variacao(produto.Id, tipo.Id, opcao.Id);
        test.Db.AddRange(tipo, opcao, variacao);
        await test.Db.SaveChangesAsync();

        var service = NewService(test);
        var request = new PedidoRequest
        {
            ClienteId = cliente.Id,
            Itens = new List<ItemPedidoRequest>
            {
                new() { ProdutoId = produto.Id, VariacaoId = variacao.Id, Quantidade = 1 }
            }
        };

        var result = await service.CreateAsync(request);

        result.IsSuccess.Should().BeTrue(result.Message);
        result.Data!.Itens[0].VariacaoId.Should().Be(variacao.Id);
        result.Data.Itens[0].VariacaoDescricao.Should().Be("Cor: Vermelho");
        result.Data.Itens[0].ProdutoNome.Should().Be("Produto X");
    }

    [Fact]
    public async Task CreateAsync_RetornaErro_QuandoVariacaoNaoExiste()
    {
        await using var test = new TestDbContext();
        var (cliente, produto, _) = await SeedClienteEProdutoAsync(test);

        var service = NewService(test);
        var idInexistente = Guid.NewGuid();
        var request = new PedidoRequest
        {
            ClienteId = cliente.Id,
            Itens = new List<ItemPedidoRequest>
            {
                new() { ProdutoId = produto.Id, VariacaoId = idInexistente, Quantidade = 1 }
            }
        };

        var result = await service.CreateAsync(request);

        result.IsSuccess.Should().BeFalse();
        result.Message.Should().ContainEquivalentOf("não encontrada");
    }

    [Fact]
    public async Task CreateAsync_ComMultiplosItens_CalculaValorTotalSomandoSubtotais()
    {
        await using var test = new TestDbContext();
        var (cliente, produto, _) = await SeedClienteEProdutoAsync(test, preco: 10m, quantidadeEstoque: 100);
        var produto2 = new Produto("Produto Y", produto.CategoriaId, produto.CatalogoId, 7m, null, "");
        var estoque2 = new Estoque(produto2.Id, 50) { Disponivel = true, Quantidade = 50 };
        test.Db.Produtos.Add(produto2);
        test.Db.Estoques.Add(estoque2);
        await test.Db.SaveChangesAsync();

        var service = NewService(test);
        var request = new PedidoRequest
        {
            ClienteId = cliente.Id,
            Itens = new List<ItemPedidoRequest>
            {
                new() { ProdutoId = produto.Id, Quantidade = 2 },
                new() { ProdutoId = produto2.Id, Quantidade = 5 }
            }
        };

        var result = await service.CreateAsync(request);

        result.IsSuccess.Should().BeTrue(result.Message);
        result.Data!.ValorTotal.Should().Be(2 * 10m + 5 * 7m);
    }

    [Fact]
    public async Task CreateAsync_UsaPrecoComDesconto_QuandoProdutoTem()
    {
        await using var test = new TestDbContext();
        var (cliente, produto, _) = await SeedClienteEProdutoAsync(test, preco: 100m, precoComDesconto: 80m, quantidadeEstoque: 5);

        var service = NewService(test);
        var request = new PedidoRequest
        {
            ClienteId = cliente.Id,
            Itens = new List<ItemPedidoRequest> { new() { ProdutoId = produto.Id, Quantidade = 1 } }
        };

        var result = await service.CreateAsync(request);

        result.IsSuccess.Should().BeTrue(result.Message);
        result.Data!.ValorTotal.Should().Be(80m);
    }

    [Fact]
    public async Task CreateAsync_RetornaErro_QuandoClienteNaoExiste()
    {
        await using var test = new TestDbContext();
        var service = NewService(test);
        var request = new PedidoRequest
        {
            ClienteId = Guid.NewGuid(),
            Itens = new List<ItemPedidoRequest> { new() { ProdutoId = Guid.NewGuid(), Quantidade = 1 } }
        };

        var result = await service.CreateAsync(request);

        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Cliente não encontrado");
    }

    [Fact]
    public async Task CreateAsync_RetornaErro_QuandoItensNull()
    {
        await using var test = new TestDbContext();
        var (cliente, _, _) = await SeedClienteEProdutoAsync(test);
        var service = NewService(test);
        var request = new PedidoRequest { ClienteId = cliente.Id, Itens = null };

        var result = await service.CreateAsync(request);

        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("O pedido deve conter pelo menos um item");
    }

    [Fact]
    public async Task CreateAsync_RetornaErro_QuandoItensVazio()
    {
        await using var test = new TestDbContext();
        var (cliente, _, _) = await SeedClienteEProdutoAsync(test);
        var service = NewService(test);
        var request = new PedidoRequest { ClienteId = cliente.Id, Itens = new List<ItemPedidoRequest>() };

        var result = await service.CreateAsync(request);

        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("O pedido deve conter pelo menos um item");
    }

    [Fact]
    public async Task CreateAsync_RetornaErro_QuandoProdutoNaoExiste()
    {
        await using var test = new TestDbContext();
        var (cliente, _, _) = await SeedClienteEProdutoAsync(test);
        var service = NewService(test);
        var idInexistente = Guid.NewGuid();
        var request = new PedidoRequest
        {
            ClienteId = cliente.Id,
            Itens = new List<ItemPedidoRequest> { new() { ProdutoId = idInexistente, Quantidade = 1 } }
        };

        var result = await service.CreateAsync(request);

        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain(idInexistente.ToString());
    }

    [Fact]
    public async Task CreateAsync_RetornaErro_QuandoEstoqueInexistente()
    {
        await using var test = new TestDbContext();
        var catalogo = NovoCatalogo();
        var categoria = NovaCategoria(catalogo.Id);
        var produto = new Produto("Prod", categoria.Id, catalogo.Id, 10m, null, "");
        var cliente = NovoCliente("F", "f@x.com");
        test.Db.AddRange(catalogo, categoria, produto, cliente);
        await test.Db.SaveChangesAsync();

        var service = NewService(test);
        var request = new PedidoRequest
        {
            ClienteId = cliente.Id,
            Itens = new List<ItemPedidoRequest> { new() { ProdutoId = produto.Id, Quantidade = 1 } }
        };

        var result = await service.CreateAsync(request);

        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("esgotado");
    }

    [Fact]
    public async Task CreateAsync_RetornaErro_QuandoEstoqueIndisponivel()
    {
        await using var test = new TestDbContext();
        var (cliente, produto, _) = await SeedClienteEProdutoAsync(test, disponivel: false);

        var service = NewService(test);
        var request = new PedidoRequest
        {
            ClienteId = cliente.Id,
            Itens = new List<ItemPedidoRequest> { new() { ProdutoId = produto.Id, Quantidade = 1 } }
        };

        var result = await service.CreateAsync(request);

        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("não está disponível");
    }

    [Fact]
    public async Task CreateAsync_RetornaErro_QuandoQuantidadeInsuficiente()
    {
        await using var test = new TestDbContext();
        var (cliente, produto, _) = await SeedClienteEProdutoAsync(test, quantidadeEstoque: 2);

        var service = NewService(test);
        var request = new PedidoRequest
        {
            ClienteId = cliente.Id,
            Itens = new List<ItemPedidoRequest> { new() { ProdutoId = produto.Id, Quantidade = 5 } }
        };

        var result = await service.CreateAsync(request);

        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("Quantidade insuficiente");
    }

    [Fact]
    public async Task GetByIdAsync_RetornaPedido_QuandoExiste()
    {
        await using var test = new TestDbContext();
        var (cliente, produto, _) = await SeedClienteEProdutoAsync(test);
        var service = NewService(test);
        var created = await service.CreateAsync(new PedidoRequest
        {
            ClienteId = cliente.Id,
            Itens = new List<ItemPedidoRequest> { new() { ProdutoId = produto.Id, Quantidade = 1 } }
        });

        var result = await service.GetByIdAsync(created.Data!.Id);

        result.IsSuccess.Should().BeTrue(result.Message);
        result.Data!.Id.Should().Be(created.Data.Id);
        result.Data.ClienteNome.Should().Be("Fulano");
        result.Data.Itens.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetByIdAsync_RetornaErro_QuandoNaoExiste()
    {
        await using var test = new TestDbContext();
        var service = NewService(test);

        var result = await service.GetByIdAsync(Guid.NewGuid());

        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Pedido não encontrado");
    }

    [Fact]
    public async Task GetAllAsync_RetornaTodosOsPedidos()
    {
        await using var test = new TestDbContext();
        var (cliente, produto, _) = await SeedClienteEProdutoAsync(test, quantidadeEstoque: 10);
        var service = NewService(test);
        await service.CreateAsync(new PedidoRequest { ClienteId = cliente.Id, Itens = new() { new() { ProdutoId = produto.Id, Quantidade = 1 } } });
        await service.CreateAsync(new PedidoRequest { ClienteId = cliente.Id, Itens = new() { new() { ProdutoId = produto.Id, Quantidade = 2 } } });

        var result = await service.GetAllAsync();

        result.IsSuccess.Should().BeTrue(result.Message);
        result.Data.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByClienteAsync_FiltraPorCliente()
    {
        await using var test = new TestDbContext();
        var (cliente, produto, _) = await SeedClienteEProdutoAsync(test, quantidadeEstoque: 10);
        var outroCliente = NovoCliente("Beltrano", "b@x.com");
        test.Db.Clientes.Add(outroCliente);
        await test.Db.SaveChangesAsync();
        var service = NewService(test);
        await service.CreateAsync(new PedidoRequest { ClienteId = cliente.Id, Itens = new() { new() { ProdutoId = produto.Id, Quantidade = 1 } } });
        await service.CreateAsync(new PedidoRequest { ClienteId = outroCliente.Id, Itens = new() { new() { ProdutoId = produto.Id, Quantidade = 1 } } });

        var result = await service.GetByClienteAsync(cliente.Id);

        result.IsSuccess.Should().BeTrue(result.Message);
        result.Data.Should().HaveCount(1);
        result.Data![0].ClienteId.Should().Be(cliente.Id);
    }

    [Fact]
    public async Task DeleteAsync_RemovePedidoERestauraEstoque()
    {
        await using var test = new TestDbContext();
        var (cliente, produto, _) = await SeedClienteEProdutoAsync(test, quantidadeEstoque: 10);
        var service = NewService(test);
        var created = await service.CreateAsync(new PedidoRequest
        {
            ClienteId = cliente.Id,
            Itens = new() { new() { ProdutoId = produto.Id, Quantidade = 4 } }
        });
        // Estoque fica em 6 após criação
        (await test.Db.Estoques.AsNoTracking().FirstAsync()).Quantidade.Should().Be(6);

        var result = await service.DeleteAsync(created.Data!.Id);

        result.IsSuccess.Should().BeTrue(result.Message);
        result.Data.Should().BeTrue();
        test.Db.Pedidos.Should().BeEmpty();
        (await test.Db.Estoques.AsNoTracking().FirstAsync()).Quantidade.Should().Be(10);
    }

    [Fact]
    public async Task DeleteAsync_NaoRestauraQuantidade_QuandoEstoqueIlimitado()
    {
        await using var test = new TestDbContext();
        var (cliente, produto, _) = await SeedClienteEProdutoAsync(test, quantidadeEstoque: null);
        var service = NewService(test);
        var created = await service.CreateAsync(new PedidoRequest
        {
            ClienteId = cliente.Id,
            Itens = new() { new() { ProdutoId = produto.Id, Quantidade = 7 } }
        });
        created.IsSuccess.Should().BeTrue();

        var result = await service.DeleteAsync(created.Data!.Id);

        result.IsSuccess.Should().BeTrue(result.Message);
        var estoque = await test.Db.Estoques.AsNoTracking().FirstAsync();
        estoque.Quantidade.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_RetornaErro_QuandoNaoExiste()
    {
        await using var test = new TestDbContext();
        var service = NewService(test);

        var result = await service.DeleteAsync(Guid.NewGuid());

        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Pedido não encontrado");
    }
}
