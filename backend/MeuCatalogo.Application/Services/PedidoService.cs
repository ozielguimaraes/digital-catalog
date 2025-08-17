using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using MeuCatalogo.Application.DTOs.Requests;
using MeuCatalogo.Application.DTOs.Responses;
using MeuCatalogo.Application.Entities;
using MeuCatalogo.Application.Interfaces;
using MeuCatalogo.Application.Infrastructure.Data;
using MeuCatalogo.Application.Infrastructure.Data.Repository;
using MeuCatalogo.Application.Infrastructure.Mappers;
using Microsoft.Extensions.Logging;

namespace MeuCatalogo.Application.Services;

public sealed class PedidoService : IPedidoService
{
    private readonly ILogger<PedidoService> _logger;
    private readonly ApplicationDbContext _dbContext;

    public PedidoService(ApplicationDbContext dbContext, ILogger<PedidoService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<ApiResponse<PedidoResponse>> CreateAsync(PedidoRequest request)
    {
        _logger.LogInformation("Criando Pedido {Payload}", JsonSerializer.Serialize(request));

        await using var transaction = await _dbContext.Database.BeginTransactionAsync();

        try
        {
            var cliente = await _dbContext.Clientes.FindAsync(request.ClienteId);
            if (cliente == null)
                return ApiResponse<PedidoResponse>.Error("Cliente não encontrado");

            if (request.Itens == null || request.Itens.Count == 0)
                return ApiResponse<PedidoResponse>.Error("O pedido deve conter pelo menos um item");

            var pedido = new Pedido(clienteId: request.ClienteId);

            await _dbContext.Pedidos.AddAsync(pedido);
            await _dbContext.SaveChangesAsync();

            foreach (var itemRequest in request.Itens)
            {
                var produto = await _dbContext.Produtos.FindAsync(itemRequest.ProdutoId);
                if (produto == null)
                {
                    _logger.LogWarning("Produto com ID {ProdutoId} não encontrado", itemRequest.ProdutoId);
                    return ApiResponse<PedidoResponse>.Error($"Produto com ID {itemRequest.ProdutoId} não encontrado");
                }
                var estoque = await _dbContext.Estoques
                    .FirstOrDefaultAsync(e => e.ProdutoId == itemRequest.ProdutoId);

                if (estoque == null)
                {
                    _logger.LogWarning("Estoque para o produto {ProdutoId} não encontrado", itemRequest.ProdutoId);
                    return ApiResponse<PedidoResponse>.Error($"Produto {produto.Nome} esgotado");
                }

                if (!estoque.Disponivel)
                    return ApiResponse<PedidoResponse>.Error($"Produto {produto.Nome} não está disponível");

                if (!estoque.TemEstoqueSuficiente(itemRequest.Quantidade))
                    return ApiResponse<PedidoResponse>.Error($"Quantidade insuficiente do produto {produto.Nome} em estoque");

                decimal precoUnitario = produto.ObterPrecoUnitario();

                var item = new ItemPedido(
                    pedidoId: pedido.Id,
                    produtoId: itemRequest.ProdutoId,
                    quantidade: itemRequest.Quantidade,
                    precoUnitario: precoUnitario,
                    variacaoId: null
                );

                await _dbContext.ItensPedido.AddAsync(item);

                estoque.DecrementarEstoque(itemRequest.Quantidade);
                _dbContext.Estoques.Update(estoque);
            }

            await _dbContext.SaveChangesAsync();

            pedido.CalcularValorTotal();
            _dbContext.Pedidos.Update(pedido);
            await _dbContext.SaveChangesAsync();

            await transaction.CommitAsync();

            var pedidoCompleto = await _dbContext.ObterPedidoPorIdComItensAsync(pedido.Id);

            return ApiResponse<PedidoResponse>.Success(pedidoCompleto!.MapToResponse());
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return ApiResponse<PedidoResponse>.Error($"Erro ao criar pedido: {ex.Message}");
        }
    }

    public async Task<ApiResponse<PedidoResponse>> GetByIdAsync(Guid id)
    {
        try
        {
            var pedido = await _dbContext.Pedidos
                .Include(p => p.Cliente)
                .Include(p => p.Itens)
                .ThenInclude(i => i.Produto)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pedido == null)
                return ApiResponse<PedidoResponse>.Error("Pedido não encontrado");

            return ApiResponse<PedidoResponse>.Success(pedido.MapToResponse());
        }
        catch (Exception ex)
        {
            return ApiResponse<PedidoResponse>.Error($"Erro ao buscar pedido: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<PedidoResponse>>> GetAllAsync()
    {
        try
        {
            var pedidos = await _dbContext.Pedidos
                .Include(p => p.Cliente)
                .Include(p => p.Itens)
                .ThenInclude(i => i.Produto)
                .ToListAsync();


            var pedidosResposta = pedidos.MapToResponse();
            return ApiResponse<List<PedidoResponse>>.Success(pedidosResposta);
        }
        catch (Exception ex)
        {
            return ApiResponse<List<PedidoResponse>>.Error($"Erro ao buscar pedidos: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<PedidoResponse>>> GetByClienteAsync(Guid clienteId)
    {
        try
        {
            var pedidos = await _dbContext.Pedidos
                .Include(p => p.Cliente)
                .Include(p => p.Itens)
                .ThenInclude(i => i.Produto)
                .Where(p => p.ClienteId == clienteId)
                .ToListAsync();

            var pedidosResposta = pedidos.MapToResponse();
            return ApiResponse<List<PedidoResponse>>.Success(pedidosResposta);
        }
        catch (Exception ex)
        {
            return ApiResponse<List<PedidoResponse>>.Error($"Erro ao buscar pedidos do cliente: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> DeleteAsync(Guid id)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync();

        try
        {
            var pedido = await _dbContext.Pedidos
                .Include(p => p.Itens)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pedido == null)
                return ApiResponse<bool>.Error("Pedido não encontrado");

            foreach (var item in pedido.Itens)
            {
                var estoque = await _dbContext.Estoques
                    .FirstOrDefaultAsync(e => e.ProdutoId == item.ProdutoId);

                if (estoque == null)
                    return ApiResponse<bool>.Error("");

                if (estoque.EhIlimitado())
                {
                    continue;
                }

                estoque.IncrementarEstoque(item.Quantidade);
                _dbContext.Estoques.Update(estoque);
            }

            _dbContext.ItensPedido.RemoveRange(pedido.Itens);
            _dbContext.Pedidos.Remove(pedido);

            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            return ApiResponse<bool>.Success(true, "Pedido removido com sucesso");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return ApiResponse<bool>.Error($"Erro ao remover pedido: {ex.Message}");
        }
    }
}
