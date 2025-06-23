using Microsoft.EntityFrameworkCore;
using MeuCatalogo.Application.Entities;

namespace MeuCatalogo.Application.Infrastructure.Data.Repository;

public static class PedidoRepositoryExtensions
{
    public static async Task<Pedido?> ObterPedidoPorIdAsync(this ApplicationDbContext context, Guid id)
    {
        return await context.Pedidos.FindAsync(id);
    }

    public static async Task<Pedido?> ObterPedidoPorIdComItensAsync(this ApplicationDbContext context, Guid id)
    {
        return await context.Pedidos
            .Include(p => p.Cliente)
            .Include(p => p.Itens)
            .ThenInclude(i => i.Produto)
            .ThenInclude(x=>x.Variacoes)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public static async Task<List<Pedido>> GetAllPedidosAsync(this ApplicationDbContext context, bool includeDetails = false)
    {
        if (includeDetails)
        {
            return await context.Pedidos
                .Include(p => p.Cliente)
                .Include(p => p.Itens)
                .ThenInclude(i => i.Produto)
                .ToListAsync();
        }
        else
        {
            return await context.Pedidos.ToListAsync();
        }
    }

    public static async Task<List<Pedido>> GetPedidosByClienteIdAsync(this ApplicationDbContext context, Guid clienteId, bool includeDetails = false)
    {
        if (includeDetails)
        {
            return await context.Pedidos
                .Include(p => p.Cliente)
                .Include(p => p.Itens)
                .ThenInclude(i => i.Produto)
                .Where(p => p.ClienteId == clienteId)
                .ToListAsync();
        }
        else
        {
            return await context.Pedidos
                .Where(p => p.ClienteId == clienteId)
                .ToListAsync();
        }
    }

    public static async Task<Estoque?> GetEstoqueByProdutoIdAsync(this ApplicationDbContext context, Guid produtoId)
    {
        return await context.Estoques.FirstOrDefaultAsync(e => e.ProdutoId == produtoId);
    }

    public static async Task AddPedidoAsync(this ApplicationDbContext context, Pedido pedido)
    {
        await context.Pedidos.AddAsync(pedido);
        await context.SaveChangesAsync();
    }

    public static async Task AddItemPedidoAsync(this ApplicationDbContext context, ItemPedido item)
    {
        await context.ItensPedido.AddAsync(item);
        await context.SaveChangesAsync();
    }

    public static async Task UpdatePedidoAsync(this ApplicationDbContext context, Pedido pedido)
    {
        context.Pedidos.Update(pedido);
        await context.SaveChangesAsync();
    }

    public static async Task DeletePedidoAsync(this ApplicationDbContext context, Guid id)
    {
        var pedido = await context.Pedidos
            .Include(p => p.Itens)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (pedido != null)
        {
            context.ItensPedido.RemoveRange(pedido.Itens);
            context.Pedidos.Remove(pedido);
            await context.SaveChangesAsync();
        }
    }
}
